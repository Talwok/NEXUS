using System.Collections.Generic;
using DynamicData;
using NEXUS.Fractal.ViewModels;
using System;
using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.Services;

public class ChartService : ServiceBase
{
    private readonly SourceCache<ChartViewModel, string> _sourceCache;

    public ChartService()
    {
        _sourceCache = new SourceCache<ChartViewModel, string>(chart => chart.Path);
        _sourceCache.Connect()
            .Bind(out var charts)
            .Subscribe();

        Charts = charts;
    }

    public ReadOnlyObservableCollection<ChartViewModel> Charts { get; }
    
    [Reactive]
    public ChartViewModel? SelectedChart { get; set; }
    
    public void Clear()
    {
        _sourceCache.Clear();
    }

    public void Add(ImageFileViewModel image, IDictionary<double, double> values)
    {
        var chart = new ChartViewModel(image, values);
        _sourceCache.AddOrUpdate(chart);
    }
}