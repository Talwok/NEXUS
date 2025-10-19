using System.Collections.Generic;
using NEXUS.Fractal.Helpers;
using NEXUS.Fractal.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.Services;
using NEXUS.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class FractalDimensionResearchViewModel : ResearchViewModel
{
    private readonly LineSeries<Point> _series;
    private readonly ProjectService? _projectService;

    public FractalDimensionResearchViewModel(FractalDimensionResearchModel model)
    {
        var statefulServices = App.ServiceProvider.GetServices<StatefulServiceBase>();
        _projectService = statefulServices.FirstOrDefault<ProjectService>();

        ParentId = model.ParentId;
        ParentName = _projectService?.Project?.Frames.FirstOrDefault(f => f.Id == ParentId)?.Name;
        Name = model.Name;
        DimensionType = model.DimensionType;
        X = model.X;
        Y = model.Y;

        (MinX, MaxX) = X.GetMinMax();
        (MinY, MaxY) = Y.GetMinMax();

        LowerLimitX = float.IsNaN(model.LowerLimitX) ? MinX : model.LowerLimitX;
        UpperLimitX = float.IsNaN(model.UpperLimitX) ? MaxX : model.UpperLimitX;
        LowerLimitY = float.IsNaN(model.LowerLimitY) ? MinY : model.LowerLimitY;
        UpperLimitY = float.IsNaN(model.UpperLimitY) ? MaxY : model.UpperLimitY;

        LowerLimitX = 2.5f;
        UpperLimitX = 3.5f;

        var valueList = new ObservableCollection<Point>();

        _series = new LineSeries<Point>
        {
            Name = "Расчетный диапазон",
            Mapping = (coord, _) => new Coordinate(coord.X, coord.Y),
            Values = valueList,
            Stroke = new SolidColorPaint(NexusColors.AccentSkColor, 3),
            Fill = null,
            GeometrySize = 5,
            GeometryFill = new SolidColorPaint(NexusColors.AccentSkColor),
            GeometryStroke = new SolidColorPaint(NexusColors.AccentSkColor, 3),
            AnimationsSpeed = null
        };

        this.WhenAnyValue(
                vm => vm.LowerLimitX,
                vm => vm.UpperLimitX,
                vm => vm.LowerLimitY,
                vm => vm.UpperLimitY)
            .Subscribe(tuple =>
            {
                var (lowerLimitX, upperLimitX, lowerLimitY, upperLimitY) = tuple;

                valueList.Clear();

                for (var i = 0; i < X.Count; i++)
                {
                    var x = X[i];
                    var y = Y[i];
                    if (x >= lowerLimitX && x <= upperLimitX
                        && y >= lowerLimitY && y <= upperLimitY)
                    {
                        valueList.Add(new Point(x, y));
                    }
                }

                var dictionary = valueList.ToDictionary(p => (float)p.X, p => (float)p.Y);

                if (FractalDimensionHelper.CalculateDimension(dictionary.Keys.ToList(), dictionary.Values.ToList(),
                        DimensionType) is { } dimension)
                {
                    Dimension = dimension;
                }
            });

        Series = [_series];

    }

    [Reactive] public string? ParentName { get; set; }
    [Reactive] public Guid ParentId { get; set; }
    [Reactive] public string? Name { get; set; }

    [Reactive] public IEnumerable<ISeries> Series { get; set; }

    [Reactive] public FractalDimensionType DimensionType { get; set; }
    [Reactive] public List<float> X { get; set; }
    [Reactive] public List<float> Y { get; set; }
    [Reactive] public float LowerLimitX { get; set; }
    [Reactive] public float UpperLimitX { get; set; }
    [Reactive] public float LowerLimitY { get; set; }
    [Reactive] public float UpperLimitY { get; set; }
    [Reactive] public float Dimension { get; set; }
    [Reactive] public float MinX { get; set; }
    [Reactive] public float MinY { get; set; }
    [Reactive] public float MaxX { get; set; }
    [Reactive] public float MaxY { get; set; }

    public override ResearchModel GetModel()
        => new FractalDimensionResearchModel
        {
            ParentId = ParentId,
            Name = Name,
            DimensionType = DimensionType,
            X = X,
            Y = Y,
            LowerLimitX = LowerLimitX,
            UpperLimitX = UpperLimitX,
            LowerLimitY = LowerLimitY,
            UpperLimitY = UpperLimitY,
        };
}

public static class FractalDim
{
    public static (float Min, float Max) GetMinMax(this List<float> list)
    {
        return (list.Min(), list.Max());
    }
}