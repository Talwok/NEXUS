using System.Collections.Generic;
using NEXUS.Fractal.Enums;

namespace NEXUS.Fractal.Models;

public class FractalDimensionModel
{
    public FractalDimensionType Type { get; set; }
    public List<float> X { get; set; } = [];
    public List<float> Y { get; set; } = [];

}