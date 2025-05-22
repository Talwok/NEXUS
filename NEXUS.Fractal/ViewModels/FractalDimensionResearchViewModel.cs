using System.Collections.Generic;
using NEXUS.Fractal.Models;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class FractalDimensionResearchViewModel : ResearchViewModel
{
    public FractalDimensionResearchViewModel(FractalDimensionResearchModel model)
    {
        X = model.X;
        Y = model.Y;
        Dimension = model.Dimension;
    }
    
    [Reactive] public List<float> X { get; set; }
    [Reactive] public List<float> Y { get; set; }
    [Reactive] public float Dimension { get; set; }
    
    public override ResearchModel GetModel()
        => new FractalDimensionResearchModel
        {
            X = X,
            Y = Y,
            Dimension = Dimension
        };
}