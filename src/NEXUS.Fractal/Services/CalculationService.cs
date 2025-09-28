using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NEXUS.Extensions;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.Helpers;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.Services
{
    public class CalculationService : ServiceBase
    {
        private readonly ProjectService? _projectService;

        public CalculationService(IEnumerable<StatefulServiceBase> statefulServices, InfoService infoService)
        {
            _projectService = statefulServices.FirstOrDefault<ProjectService>();
        }

        [Reactive] public bool IsCalculating { get; set; }

        public void CalculateDimension(FractalDimensionType fractalDimensionType)
        {
            IsCalculating = true;
                
            foreach (var frame in _projectService?.SelectedFrames)
            {
                if (frame == null) continue;
                
                var model = fractalDimensionType switch
                {
                    FractalDimensionType.BoxCountingFractalDimension => frame.HeightMap.CalculateBoxCountingDimension(),
                    FractalDimensionType.VarianceFractalDimension => frame.HeightMap.CalculateVarianceDimension(),
                    FractalDimensionType.TriangulationFractalDimension => frame.HeightMap.CalculateTriangulationDimension(),
                    _ => null
                };

                if (model != null)
                {
                    FractalDimensionResearchModel researchModel = new()
                    {
                        ParentId = frame.Id,
                        Name = FractalDimensionHelper.GetDimensionName(model.Type),
                        DimensionType = model.Type,
                        X = model.X,
                        Y = model.Y
                    };
                    _projectService?.Project?.Researches.Add(new FractalDimensionResearchViewModel(researchModel));
                }   
            }

            IsCalculating = false;
        }
    }
}