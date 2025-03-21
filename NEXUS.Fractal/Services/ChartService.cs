using System.Collections.Generic;
using DynamicData;
using NEXUS.Fractal.ViewModels;

namespace NEXUS.Fractal.Services;

public class ChartService : ServiceBase
{
    private readonly SourceCache<ChartViewModel, string> _sourceCache;

    public ChartService()
    {
        _sourceCache = new SourceCache<ChartViewModel, string>(chart => chart.Path);
    }
    
    public void Clear()
    {
        _sourceCache.Clear();
    }

    public void Add(string path, IDictionary<double, double> values)
    {
        var chart = new ChartViewModel
        {
            Path = path,
            Values = values
        };
        _sourceCache.AddOrUpdate(chart);
    }
}