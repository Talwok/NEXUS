using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NEXUS.Extensions;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.Helpers;
using NEXUS.Fractal.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.Services;

public class GeometryService : ServiceBase
{
    private readonly ProjectService? _projectService;

    public GeometryService(IEnumerable<StatefulServiceBase> statefulServices, InfoService infoService)
    {
        _projectService = statefulServices.FirstOrDefault<ProjectService>();
    }
    
    [Reactive] public bool IsCalculating { get; set; }
    
    public void UpdateGeometry(GeometryUpdateType updateType)
    {
        IsCalculating = true;

        if (_projectService?.SelectedItem is FrameViewModel frame)
        {
            var heightMap = updateType switch
            {
                GeometryUpdateType.RemoveLinearTrend => frame.HeightMap.RemoveLinearTrend(),
                GeometryUpdateType.RemoveQuadraticTrend => frame.HeightMap.RemoveQuadraticTrend(),
                GeometryUpdateType.LocalAlignment => frame.HeightMap.LocalAlignment(),
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
                _projectService.SelectedItem = frameVm;
            }
        }
        
        IsCalculating = false;
    }
    
}