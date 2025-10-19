using System;
using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using NEXUS.Parsers.MDT.Models.Pallete;
using Silk.NET.OpenGL;

namespace NEXUS.Fractal.Controls.Surface;

internal partial class SurfaceOpenGlControl
{
    private GL _gl;

    private BufferObject<float>? _axisVbo;
    private VertexArrayObject<float, uint>? _axisVao;
    private Shader? _axisShader;
    private uint _axisTexture;

    // Camera parameters
    private static readonly Vector3 CameraStartPosition = new(0, 0, 75);
    private Vector3 _cameraPosition = CameraStartPosition;
    private readonly Vector3 _cameraTarget = Vector3.Zero;
    private readonly Color _foundationColor = Colors.LightGray;
    private readonly float _minZoom = 1;
    private readonly float _maxZoom = 300;

    private Matrix4x4 _modelMatrix = Matrix4x4.Identity;
    private float _modelYaw;
    private float _modelPitch = 1.5f;
    private float _heightMultiplier = 1f;
    private bool _showFoundation = true;
    private int _indicesCount;
    private Color _backgroundColor;
    private float _ambientStrength = 0.3f;
    private float _specularStrength = 0.4f;
    private PaletteColorTable _colorTable;
    private float[,] _heightMap;
    private float _lightPositionX;
    private float _lightPositionY;
    private float _lightPositionZ;
    private float _zoom;
    private float _colorTableUpperSelection;
    private float _colorTableLowerSelection;

    public Bitmap Image { get; set; }

    public BufferObject<uint> EBO { get; set; }
    public BufferObject<OpenGlPoint> VBO { get; set; }
    public VertexArrayObject<OpenGlPoint, uint> VAO { get; set; }

    public Shader Shader { get; set; }


    public PaletteColorTable ColorTable
    {
        get => _colorTable;
        set
        {
            SetAndRaise(ColorTableProperty, ref _colorTable, value);
            RecreateBuffersFromHeightMap();
            UpdateRender();
        }
    }


    public float[,] HeightMap
    {
        get => _heightMap;
        set
        {
            SetAndRaise(HeightMapProperty, ref _heightMap, value);
            RecreateBuffersFromHeightMap();
            UpdateRender();
        }
    }

    public float HeightMultiplier
    {
        get => _heightMultiplier;
        set
        {
            SetAndRaise(HeightMultiplierProperty, ref _heightMultiplier, value);
            UpdateRender();
        }
    }

    public bool ShowFoundation
    {
        get => _showFoundation;
        set
        {
            SetAndRaise(ShowFoundationProperty, ref _showFoundation, value);
            UpdateRender();
        }
    }

    public float ModelPitch
    {
        get => _modelPitch;
        set
        {
            SetAndRaise(ModelPitchProperty, ref _modelPitch, value);
            UpdateRender();
        }
    }


    public float ModelYaw
    {
        get => _modelYaw;
        set
        {
            SetAndRaise(ModelYawProperty, ref _modelYaw, value);
            UpdateRender();
        }
    }

    public float LightPositionX
    {
        get => _lightPositionX;
        set
        {
            SetAndRaise(LightPositionXProperty, ref _lightPositionX, value);
            UpdateRender();
        }
    }

    public float LightPositionY
    {
        get => _lightPositionY;
        set
        {
            SetAndRaise(LightPositionYProperty, ref _lightPositionY, value);
            UpdateRender();
        }
    }

    public float LightPositionZ
    {
        get => _lightPositionZ;
        set
        {
            SetAndRaise(LightPositionZProperty, ref _lightPositionZ, value);
            UpdateRender();
        }
    }

    public float Zoom
    {
        get => _zoom;
        set
        {
            if (value < 1)
                return;

            SetAndRaise(ZoomProperty, ref _zoom, value);
            var direction = Vector3.Normalize(_cameraTarget - _cameraPosition);
            var newDistance = Math.Clamp(value, _minZoom, _maxZoom);
            _cameraPosition = _cameraTarget - direction * newDistance;
            UpdateRender();
        }
    }

    public float AmbientStrength
    {
        get => _ambientStrength;
        set
        {
            SetAndRaise(AmbientStrengthProperty, ref _ambientStrength, value);
            UpdateRender();
        }
    }

    public float SpecularStrength
    {
        get => _specularStrength;
        set
        {
            SetAndRaise(SpecularStrengthProperty, ref _specularStrength, value);
            UpdateRender();
        }
    }

    public float ColorTableUpperSelection
    {
        get => _colorTableUpperSelection;
        set => SetAndRaise(ColorTableUpperSelectionProperty, ref _colorTableUpperSelection, value);
    }

    public float ColorTableLowerSelection
    {
        get => _colorTableLowerSelection;
        set => SetAndRaise(ColorTableLowerSelectionProperty, ref _colorTableLowerSelection, value);
    }

    public static readonly DirectProperty<SurfaceOpenGlControl, bool> ShowFoundationProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, bool>(
            nameof(ShowFoundation), o => o.ShowFoundation, (o, v) => o.ShowFoundation = v);

    public static readonly DirectProperty<SurfaceOpenGlControl, float> HeightMultiplierProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, float>(
            nameof(HeightMultiplier), o => o.HeightMultiplier, (o, v) => o.HeightMultiplier = v);

    public static readonly DirectProperty<SurfaceOpenGlControl, float> ModelPitchProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, float>(
            nameof(ModelPitch), o => o.ModelPitch, (o, v) => o.ModelPitch = v);

    public static readonly DirectProperty<SurfaceOpenGlControl, float> ModelYawProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, float>(
            nameof(ModelYaw), o => o.ModelYaw, (o, v) => o.ModelYaw = v);

    public static readonly DirectProperty<SurfaceOpenGlControl, float> LightPositionXProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, float>(
            nameof(LightPositionX), o => o.LightPositionX, (o, v) => o.LightPositionX = v);

    public static readonly DirectProperty<SurfaceOpenGlControl, float> LightPositionYProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, float>(
            nameof(LightPositionY), o => o.LightPositionY, (o, v) => o.LightPositionY = v);

    public static readonly DirectProperty<SurfaceOpenGlControl, float> LightPositionZProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, float>(
            nameof(LightPositionZ), o => o.LightPositionZ, (o, v) => o.LightPositionZ = v);

    public static readonly DirectProperty<SurfaceOpenGlControl, float> ZoomProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, float>(
            nameof(Zoom), o => o.Zoom, (o, v) => o.Zoom = v);

    public static readonly DirectProperty<SurfaceOpenGlControl, float> AmbientStrengthProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, float>(
            nameof(AmbientStrength), o => o.AmbientStrength, (o, v) => o.AmbientStrength = v);

    public static readonly DirectProperty<SurfaceOpenGlControl, float> SpecularStrengthProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, float>(
            nameof(SpecularStrength), o => o.SpecularStrength, (o, v) => o.SpecularStrength = v);

    public static readonly DirectProperty<SurfaceOpenGlControl, float> ColorTableUpperSelectionProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, float>(
            nameof(ColorTableUpperSelection), o => o.ColorTableUpperSelection,
            (o, v) => o.ColorTableUpperSelection = v);

    public static readonly DirectProperty<SurfaceOpenGlControl, float> ColorTableLowerSelectionProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, float>(
            nameof(ColorTableLowerSelection), o => o.ColorTableLowerSelection,
            (o, v) => o.ColorTableLowerSelection = v);

    // ColorTable
    public static readonly DirectProperty<SurfaceOpenGlControl, PaletteColorTable> ColorTableProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, PaletteColorTable>(
            nameof(ColorTable),
            o => o.ColorTable,
            (o, v) => o.ColorTable = v,
            enableDataValidation: false);

    public static readonly DirectProperty<SurfaceOpenGlControl, float[,]> HeightMapProperty =
        AvaloniaProperty.RegisterDirect<SurfaceOpenGlControl, float[,]>(
            nameof(HeightMap),
            o => o.HeightMap,
            (o, v) => o.HeightMap = v);
}