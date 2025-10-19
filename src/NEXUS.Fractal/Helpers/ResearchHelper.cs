using System.Collections.Generic;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.ViewModels;

namespace NEXUS.Fractal.Helpers;

public static class ResearchHelper
{
    public static IEnumerable<ResearchViewModel> Transform(this IEnumerable<ResearchModel> models)
    {
        foreach (var model in models)
        {
            if (model is FractalDimensionResearchModel dimensionModel)
                yield return new FractalDimensionResearchViewModel(dimensionModel);


        }
    }
}