using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.Models;

namespace NEXUS.Fractal.Controls.ViewCube
{
    public class ViewCubeControl : Control
    {
        public static readonly StyledProperty<SolidColorBrush> FrontFaceBrushProperty =
            AvaloniaProperty.Register<ViewCubeControl, SolidColorBrush>(nameof(FrontFaceBrush), new SolidColorBrush(Brushes.DodgerBlue.Color, 0.5));

        public static readonly StyledProperty<SolidColorBrush> TopFaceBrushProperty =
            AvaloniaProperty.Register<ViewCubeControl, SolidColorBrush>(nameof(TopFaceBrush), new SolidColorBrush(Brushes.ForestGreen.Color, 0.5));

        public static readonly StyledProperty<SolidColorBrush> SideFaceBrushProperty =
            AvaloniaProperty.Register<ViewCubeControl, SolidColorBrush>(nameof(SideFaceBrush), new SolidColorBrush(Brushes.Crimson.Color, 0.5));

        public static readonly StyledProperty<IBrush> TextBrushProperty =
            AvaloniaProperty.Register<ViewCubeControl, IBrush>(nameof(TextBrush), Brushes.White);

        public static readonly StyledProperty<double> CubeSizeRatioProperty =
            AvaloniaProperty.Register<ViewCubeControl, double>(nameof(CubeSizeRatio), 0.7);

        public static readonly StyledProperty<IBrush> HoverBrushProperty =
            AvaloniaProperty.Register<ViewCubeControl, IBrush>(nameof(HoverBrush), new SolidColorBrush(Colors.White, 0.3));

        public event Action<AxisViewType>? ViewSelected;

        public SolidColorBrush FrontFaceBrush
        {
            get => GetValue(FrontFaceBrushProperty);
            set => SetValue(FrontFaceBrushProperty, value);
        }

        public SolidColorBrush TopFaceBrush
        {
            get => GetValue(TopFaceBrushProperty);
            set => SetValue(TopFaceBrushProperty, value);
        }

        public SolidColorBrush SideFaceBrush
        {
            get => GetValue(SideFaceBrushProperty);
            set => SetValue(SideFaceBrushProperty, value);
        }

        public IBrush TextBrush
        {
            get => GetValue(TextBrushProperty);
            set => SetValue(TextBrushProperty, value);
        }

        public double CubeSizeRatio
        {
            get => GetValue(CubeSizeRatioProperty);
            set => SetValue(CubeSizeRatioProperty, value);
        }

        public IBrush HoverBrush
        {
            get => GetValue(HoverBrushProperty);
            set => SetValue(HoverBrushProperty, value);
        }

        private Point[]? _frontFace;
        private Point[]? _topFace;
        private Point[]? _sideFace;
        private Point _isoTextPosition;
        private double _cubeSize;
        private Point _center;
        private AxisViewType? _hoveredFace;

        public ViewCubeControl()
        {
            PointerPressed += OnPointerPressed;
            PointerMoved += OnPointerMoved;
            PointerExited += OnPointerExited;
        }

        private void OnPointerExited(object? sender, PointerEventArgs e)
        {
            _hoveredFace = null;
            InvalidateVisual();
        }

        private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
        {
            _hoveredFace = null;
            InvalidateVisual();
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            CalculateCubeGeometry();
            InvalidateVisual();
        }

        private void CalculateCubeGeometry()
        {
            var size = Math.Min(Bounds.Width, Bounds.Height);
            _center = new Point(Bounds.Width / 2, Bounds.Height / 2);
            _cubeSize = size * CubeSizeRatio;

            // Изометрические смещения
            double isoX = _cubeSize * 0.3;
            double isoY = _cubeSize * 0.25;

            // Точки передней грани (в центре)
            var p0 = new Point(_center.X, _center.Y - isoY); // top front
            var p1 = new Point(_center.X + isoX, _center.Y); // right front
            var p2 = new Point(_center.X, _center.Y + isoY); // bottom front
            var p3 = new Point(_center.X - isoX, _center.Y); // left front

            // Смещения вглубь (по Z-оси) - должно быть вверх по экрану (отнимаем значение)
            var dz = isoY; // глубина по "высоте" в изометрии

            var p4 = new Point(p0.X, p0.Y - dz); // top back
            var p5 = new Point(p1.X, p1.Y - dz); // right back
            var p6 = new Point(p2.X, p2.Y - dz); // bottom back
            var p7 = new Point(p3.X, p3.Y - dz); // left back

            // Фронтальная грань (слева)
            _frontFace = [p3, p2, p6, p7];

            // Верхняя грань
            _topFace = [p4, p5, p6, p7];

            // Боковая грань (справа)
            _sideFace = [p1, p5, p6, p2];

            _isoTextPosition = new Point(_center.X, _center.Y + _cubeSize * 0.5);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (_frontFace == null || _topFace == null || _sideFace == null)
                CalculateCubeGeometry();

            var borderPen = new Pen(new SolidColorBrush(Colors.Black, 0.3), 1.5);

            // Создаем Geometry для каждой грани
            var frontGeometry = CreateFaceGeometry(_frontFace);
            var topGeometry = CreateFaceGeometry(_topFace);
            var sideGeometry = CreateFaceGeometry(_sideFace);

            // Draw faces with gradient for better 3D effect
            var frontBrush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop { Color = FrontFaceBrush.Color, Offset = 0 },
                    new GradientStop { Color = FrontFaceBrush.Color.Darken(0.2), Offset = 1 }
                }
            };

            var topBrush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop { Color = TopFaceBrush.Color, Offset = 0 },
                    new GradientStop { Color = TopFaceBrush.Color.Darken(0.3), Offset = 1 }
                }
            };

            var sideBrush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop { Color = SideFaceBrush.Color, Offset = 0 },
                    new GradientStop { Color = SideFaceBrush.Color.Darken(0.4), Offset = 1 }
                }
            };

            // Draw faces
            context.DrawGeometry(frontBrush, borderPen, frontGeometry);
            context.DrawGeometry(topBrush, borderPen, topGeometry);
            context.DrawGeometry(sideBrush, borderPen, sideGeometry);

            // Draw hover effect
            if (_hoveredFace.HasValue)
            {
                switch (_hoveredFace.Value)
                {
                    case AxisViewType.Front:
                        context.DrawGeometry(HoverBrush, null, frontGeometry);
                        break;
                    case AxisViewType.Top:
                        context.DrawGeometry(HoverBrush, null, topGeometry);
                        break;
                    case AxisViewType.Side:
                        context.DrawGeometry(HoverBrush, null, sideGeometry);
                        break;
                }
            }

            // Draw only isometric label
            DrawCenteredText(context, "ISO", _isoTextPosition, 14);
        }

        private StreamGeometry CreateFaceGeometry(Point[]? points)
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(points[0], true);
                for (int i = 1; i < points.Length; i++)
                {
                    ctx.LineTo(points[i]);
                }
                ctx.EndFigure(true);
            }
            return geometry;
        }

        private void DrawCenteredText(DrawingContext context, string text, Point center, double fontSize)
        {
            var typeface = new Typeface("Arial");
            var formatted = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                TextBrush)
            {
                TextAlignment = TextAlignment.Center
            };

            var point = new Point(center.X - formatted.Width / 2, center.Y - formatted.Height / 2);
            context.DrawText(formatted, point);
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var pos = e.GetPosition(this);

            if (IsPointInPolygon(pos, _frontFace))
                ViewSelected?.Invoke(AxisViewType.Front);
            else if (IsPointInPolygon(pos, _topFace))
                ViewSelected?.Invoke(AxisViewType.Top);
            else if (IsPointInPolygon(pos, _sideFace))
                ViewSelected?.Invoke(AxisViewType.Side);
            else if (new Rect(_isoTextPosition.X - 20, _isoTextPosition.Y - 10, 40, 20).Contains(pos))
                ViewSelected?.Invoke(AxisViewType.Isometric);

            e.Handled = true;
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            var pos = e.GetPosition(this);
            var previousHover = _hoveredFace;

            _hoveredFace = null;

            if (IsPointInPolygon(pos, _frontFace))
                _hoveredFace = AxisViewType.Front;
            else if (IsPointInPolygon(pos, _topFace))
                _hoveredFace = AxisViewType.Top;
            else if (IsPointInPolygon(pos, _sideFace))
                _hoveredFace = AxisViewType.Side;

            if (_hoveredFace != previousHover)
            {
                InvalidateVisual();
            }
        }

        private bool IsPointInPolygon(Point point, Point[]? polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }

    public static class ColorExtensions
    {
        public static Color Darken(this Color color, double factor)
        {
            return Color.FromArgb(
                color.A,
                (byte)(color.R * (1 - factor)),
                (byte)(color.G * (1 - factor)),
                (byte)(color.B * (1 - factor)));
        }
    }
}