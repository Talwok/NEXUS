using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Converters;
using NEXUS.Extensions;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.Helpers;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.Services;
using NEXUS.Parsers.MDT.Models.Pallete;
using NEXUS.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NEXUS.Fractal.ViewModels;

public class FrameViewModel : ViewModelBase
{
    private readonly ProjectService _svc;
    
    public static readonly int Tab2DIndex = 0;
    public static readonly int Tab3DIndex = 1;

    public FrameViewModel(FrameModel frame)
    {
        Id = frame.Id;
        ParentId = frame.ParentId;
        SourceType = frame.SourceType;
        Name = frame.Name;
        HeightMap = frame.HeightMap;
        HeightSpacing = frame.HeightSpacing;
        HeightScaling = frame.HeightScaling;
        MetaData = frame.MetaData;
        AmbientStrength = frame.AmbientStrength;
        SpecularStrength = frame.SpecularStrength;
        HeightMultiplier = frame.HeightMultiplier;
        LightPositionX = frame.LightPositionX;
        LightPositionY = frame.LightPositionY;
        LightPositionZ = frame.LightPositionZ;
        Zoom = frame.Zoom;
        ModelYaw = frame.ModelYaw;
        ModelPitch = frame.ModelPitch;
        ShowFoundation = frame.ShowFoundation;
        ColorTableMaximum = frame.ColorTableMaximum;
        ColorTableMinimum = frame.ColorTableMinimum;
        ColorTableUpperSelection = frame.ColorTableUpperSelection;
        ColorTableLowerSelection = frame.ColorTableLowerSelection;
        
        if (App.ServiceProvider
                .GetServices<StatefulServiceBase>()
                .FirstOrDefault<ProjectService>() is { } svc)
        {
            _svc = svc;

            _svc.WhenAnyValue(s => s.SelectedColorTable)
                .Subscribe(colorTable => ColorTable = colorTable);
        }
        
        if (ColorTableMaximum == 0 && ColorTableMinimum == 0)
        {
            var (min, max) = HeightMap.GetMinMax();
            ColorTableMaximum = ColorTableUpperSelection = max;
            ColorTableMinimum = ColorTableLowerSelection = min;    
        }
        
        
        this.WhenAnyValue(
                vm => vm.HeightMap,
                vm => vm.ColorTable)
            .Subscribe(_ =>
            {
                UpdateColorLimits();
                UpdateImage();
            });
        
        this.WhenAnyValue(
                vm => vm.ColorTableMinimum,
                vm => vm.ColorTableMaximum,
                vm => vm.ColorTableLowerSelection,
                vm => vm.ColorTableUpperSelection,
                vm => vm.ColorTable)
            .Subscribe(range =>
            {
                var (min, max, lower, upper, table) = range;
                ColorTableRange = new(max, min, upper, lower, table.Colors);
                // UpdateColorLimits();
                UpdateImage();
            });
    }
    
    [Reactive] public Guid Id { get; set; }
    [Reactive] public Guid? ParentId { get; set; }
    [Reactive] public FrameSourceType SourceType { get; set; }
    [Reactive] public string Name { get; set; }
    [Reactive] public float[,] HeightMap { get; set; }
    [Reactive] public Bitmap FrameImage { get; set; }
    [Reactive] public PaletteColorTable ColorTable { get; set; }
    [Reactive] public float HeightSpacing { get; set; }
    [Reactive] public float HeightScaling { get; set; }
    [Reactive] public Dictionary<string, string> MetaData { get; set; }
    [Reactive] public ObservableCollection<FrameViewModel> Children { get; set; } = [];
    [Reactive] public float AmbientStrength { get; set; } = 0.3f;
    [Reactive] public float SpecularStrength { get; set; } = 0.4f;
    [Reactive] public float HeightMultiplier { get; set; } = 1;
    [Reactive] public float LightPositionX { get; set; } = 0;
    [Reactive] public float LightPositionY { get; set; } = 0;
    [Reactive] public float LightPositionZ { get; set; } = 100;
    [Reactive] public float Zoom { get; set; } = 100;
    [Reactive] public float ModelYaw { get; set; }
    [Reactive] public float ModelPitch { get; set; } = 1.5f;
    [Reactive] public bool ShowFoundation { get; set; } = true;
    [Reactive] public int SelectedViewTabIndex { get; set; }
    [Reactive] public float ColorTableMaximum { get; set; }
    [Reactive] public float ColorTableMinimum { get; set; }
    [Reactive] public float ColorTableUpperSelection { get; set; }
    [Reactive] public float ColorTableLowerSelection { get; set; }
    [Reactive] public ColorTableRange ColorTableRange { get; set; }
    
    private void UpdateImage()
    {
        var image = new Image<Rgba32>(HeightMap.GetLength(0), HeightMap.GetLength(1));
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < accessor.Width; x++)
                {
                    var value = HeightMap[y, x];
                    
                    if (ColorTable != null)
                    {
                        int colorIndex;
                        if (value > ColorTableUpperSelection)
                        {
                            colorIndex = ColorTable.Colors.Count - 1;
                        }
                        else if (value < ColorTableLowerSelection)
                        {
                            colorIndex = 0;
                        }
                        else
                        {
                            value = FrameHelper.Normalize(value, ColorTableLowerSelection, ColorTableUpperSelection);
                            colorIndex = (int)(value * (ColorTable.Colors.Count - 1));
                            colorIndex = Math.Clamp(colorIndex, 0, ColorTable.Colors.Count - 1);
                        }
                        row[x] = new Rgba32(ColorTable.Colors[colorIndex].Red, ColorTable.Colors[colorIndex].Green,
                            ColorTable.Colors[colorIndex].Blue);
                    }
                    else
                    {
                        row[x] = new Rgba32(value, value, value);
                    }
                }
            }
        });
        var stream = new MemoryStream();
        image.SaveAsBmp(stream);
        stream.Seek(0, SeekOrigin.Begin);
        FrameImage = new Bitmap(stream);
        stream.Close();
    }
    
    private void UpdateColorLimits()
    {
        var lowerSelection = ColorTableLowerSelection;
        var upperSelection = ColorTableUpperSelection;
        var currentMin = ColorTableMinimum;
        var currentMax = ColorTableMaximum;
        
        var denormalizedLowerSelection = FrameHelper.Denormalize(lowerSelection, currentMin, currentMax);
        var denormalizedUpperSelection = FrameHelper.Denormalize(upperSelection, currentMin, currentMax);
        
        var (min, max) = HeightMap.GetMinMax();
        
        ColorTableLowerSelection = FrameHelper.Normalize(denormalizedLowerSelection, min, max);
        ColorTableUpperSelection = FrameHelper.Normalize(denormalizedUpperSelection, min, max);
    }
    
    
    public FrameModel GetModel()
    {
        return new FrameModel
        {
            Id = Id,
            ParentId = ParentId,
            SourceType = SourceType,
            Name = Name,
            HeightMap = HeightMap,
            HeightSpacing = HeightSpacing,
            HeightScaling = HeightScaling,
            MetaData = MetaData,
            AmbientStrength = AmbientStrength,
            SpecularStrength = SpecularStrength,
            HeightMultiplier = HeightMultiplier,
            LightPositionX = LightPositionX,
            LightPositionY = LightPositionY,
            LightPositionZ = LightPositionZ,
            Zoom = Zoom,
            ModelYaw = ModelYaw,
            ModelPitch = ModelPitch,
            ShowFoundation = ShowFoundation,
            ColorTableMaximum = ColorTableMaximum,
            ColorTableMinimum = ColorTableMinimum,
            ColorTableUpperSelection = ColorTableUpperSelection,
            ColorTableLowerSelection = ColorTableLowerSelection
        };
    }
}