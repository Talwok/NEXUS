using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;
using NEXUS.Fractal.Services;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class SettingsDialogViewModel : ViewModelBase
{
    public SettingsDialogViewModel(IEnumerable<StatefulServiceBase> statefulServices)
    {
        ColorTableService = statefulServices.FirstOrDefault<ColorTableService>();

    }

    [Reactive]
    public ColorTableService? ColorTableService { get; set; }
}