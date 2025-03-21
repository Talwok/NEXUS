using System.Collections.Generic;
using NEXUS.ViewModels;

namespace NEXUS.Fractal.ViewModels;

public class ChartViewModel : ViewModelBase
{
    public string Path { get; init; }
    public IDictionary<double, double> Values { get; init; }
}