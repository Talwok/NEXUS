using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using NEXUS.Helpers;
using NEXUS.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public record CalculationCoordinate(double X, double Y, int Index);

public class ChartViewModel : ViewModelBase
{
    private readonly SourceCache<CalculationCoordinate, int> _pairSource;
    
    public ChartViewModel(ImageFileViewModel image, IDictionary<double, double> values)
    {
        var coordinates = values.Select((value, ind) => new CalculationCoordinate(value.Key, value.Value, ind)).ToArray();
        
        _pairSource = new SourceCache<CalculationCoordinate, int>(coord => coord.Index);
        _pairSource.Connect()
            .Filter(coord =>
            {
                var (x, y, _) = coord;
                return x >= RangeFromValueX && x <= RangeToValueX && y >= RangeFromValueY && y <= RangeToValueY;
            })
            .SortBy(coord => coord.Index)
            .Bind(out var filteredCoordinates)
            .Subscribe();
        
        this.WhenAnyValue(
                vm => vm.RangeFromValueX,
                vm => vm.RangeFromValueY,
                vm => vm.RangeToValueX,
                vm => vm.RangeToValueY)
            .Subscribe(_ =>
            {
                _pairSource.Refresh();
                
                if (filteredCoordinates.Any())
                {
                    FractalDimension = Math.Round(NormalEquations2d(
                        filteredCoordinates.Select(pair => pair.X).ToArray(),
                        filteredCoordinates.Select(pair => pair.Y).ToArray())[0], 2) + 1;
                }
            });

        Name = image.Name;
        Path = image.Path;
        Values = values;

        RangeFromLimitX = RangeFromValueX = values.Keys.Min();
        RangeFromLimitY = RangeFromValueY = values.Values.Min();
        RangeToLimitX = RangeToValueX = values.Keys.Max();
        RangeToLimitY = RangeToValueY = values.Values.Max();

        Series =
        [
            new LineSeries<CalculationCoordinate>
            {
                Name = "Расчетный диапазон",
                Values = filteredCoordinates,
                Mapping = (coord, _) => new Coordinate(coord.X, coord.Y),
                Stroke = new SolidColorPaint(NexusColors.AccentSkColor, 3),
                Fill = null,
                GeometrySize = 5,
                GeometryFill = new SolidColorPaint(NexusColors.AccentSkColor),
                GeometryStroke = new SolidColorPaint(NexusColors.AccentSkColor, 3),
                AnimationsSpeed = null
            }
        ];
        XAxes =
        [
            new Axis
            {
                Name = "X",
                AnimationsSpeed = null
            }
        ];
        YAxes =
        [
            new Axis
            {
                Name = "Y",
                AnimationsSpeed = null
            }
        ];

        _pairSource.AddOrUpdate(coordinates);
    }

    [Reactive] 
    public IEnumerable<ISeries> Series { get; set; }
    [Reactive] 
    public IEnumerable<Axis> XAxes { get; set; }
    [Reactive] 
    public IEnumerable<Axis> YAxes { get; set; }
    [Reactive] 
    public string Name { get; private set; }
    [Reactive] 
    public string Path { get; private set; }
    public IDictionary<double, double> Values { get; }
    [Reactive] 
    public double FractalDimension { get; set; }
    [Reactive] 
    public double RangeFromLimitX { get; set; }
    [Reactive] 
    public double RangeToLimitX { get; set; }
    [Reactive] 
    public double RangeFromValueX { get; set; }
    [Reactive] 
    public double RangeToValueX { get; set; }
    [Reactive] 
    public double RangeFromLimitY { get; set; }
    [Reactive] 
    public double RangeToLimitY { get; set; }
    [Reactive] 
    public double RangeFromValueY { get; set; }
    [Reactive] 
    public double RangeToValueY { get; set; }

    private double[] NormalEquations2d(double[] x, double[] y)
    {
        //x^t * x
        double[,] xtx = new double[2, 2];
        for (int i = 0; i < x.Length; i++)
        {
            xtx[0, 1] += x[i];
            xtx[0, 0] += x[i] * x[i];
        }

        xtx[1, 0] = xtx[0, 1];
        xtx[1, 1] = x.Length;

        //inverse
        double[,] xtxInv = new double[2, 2];
        double d = 1 / (xtx[0, 0] * xtx[1, 1] - xtx[1, 0] * xtx[0, 1]);
        xtxInv[0, 0] = xtx[1, 1] * d;
        xtxInv[0, 1] = -xtx[0, 1] * d;
        xtxInv[1, 0] = -xtx[1, 0] * d;
        xtxInv[1, 1] = xtx[0, 0] * d;

        //times x^t
        double[,] xtxInvxt = new double[2, x.Length];
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < x.Length; j++)
            {
                xtxInvxt[i, j] = xtxInv[i, 0] * x[j] + xtxInv[i, 1];
            }
        }

        //times y
        double[] theta = new double[2];
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < x.Length; j++)
            {
                theta[i] += xtxInvxt[i, j] * y[j];
            }
        }

        return theta;
    }
}