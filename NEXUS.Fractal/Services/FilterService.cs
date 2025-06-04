using System;
using System.Collections.Generic;
using NEXUS.Extensions;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.Helpers;
using NEXUS.Fractal.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.Services;

public class FilterService : ServiceBase
{
    private readonly ProjectService? _projectService;

    public FilterService(IEnumerable<StatefulServiceBase> statefulServices, InfoService infoService)
    {
        _projectService = statefulServices.FirstOrDefault<ProjectService>();
    }

    [Reactive] public bool IsCalculating { get; set; }

    public void ApplyFilter(FilterType filterType)
    {
        IsCalculating = true;

        if (_projectService?.SelectedFrame is FrameViewModel frame)
        {
            var heightMap = filterType switch
            {
                FilterType.Gaussian => frame.HeightMap.ApplyGaussianFilter(),
                FilterType.Bilateral => frame.HeightMap.ApplyBilateralFilter(),
                FilterType.Median => frame.HeightMap.ApplyMedianFilter(),
                _ => null
            };

            if (heightMap != null)
            {
                heightMap = heightMap.Normalize();
                var frameModel = frame.GetModel();
                var frameVm = new FrameViewModel(frameModel)
                {
                    Id = Guid.NewGuid(),
                    ParentId = frame.Id,
                    HeightMap = heightMap
                };
                frame.Children.Add(frameVm);
                _projectService.SelectedFrame = frameVm;
            }

            IsCalculating = false;
        }
    }
}