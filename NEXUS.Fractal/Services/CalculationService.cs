using System.Collections.ObjectModel;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.Helpers;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.Services
{
    public class CalculationService(
        ProjectService projectService,
        InfoService infoService)
        : ServiceBase
    {
        [Reactive] public ObservableCollection<FrameViewModel> SelectedFrames { get; set; } = [];
        [Reactive] public bool IsCalculating { get; set; }

        public void CalculateDimension(FractalDimensionType fractalDimensionType)
        {
            IsCalculating = true;
            
            foreach (var frame in SelectedFrames)
            {
                FractalDimensionModel? model = fractalDimensionType switch
                {
                    FractalDimensionType.BoxCountingFractalDimension => frame.HeightMap.CalculateBoxCountingDimension(),
                    FractalDimensionType.VarianceFractalDimension => frame.HeightMap.CalculateVarianceDimension(),
                    FractalDimensionType.MassScaleFractalDimension => frame.HeightMap.CalculateMassScaleDimension(),
                    FractalDimensionType.HiguchiFractalDimension => frame.HeightMap.CalculateHiguchiDimension(),
                    FractalDimensionType.StructureFunctionFractalDimension => frame.HeightMap.CalculateStructureFunctionDimension(),
                    FractalDimensionType.TriangulationFractalDimension => frame.HeightMap.CalculateTriangulationDimension(),
                    _ => null
                };

                if (model != null)
                {
                    FractalDimensionResearchModel researchModel = new()
                    {
                        X = model.X,
                        Y = model.Y,
                        Dimension = model.Dimension
                    };
                    projectService.Project?.Researches.Add(new FractalDimensionResearchViewModel(researchModel));    
                }
            }
            
            IsCalculating = false;
        }
    }
}