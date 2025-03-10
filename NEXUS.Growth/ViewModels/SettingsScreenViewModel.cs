using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.ViewModels;

public class SettingsScreenViewModel : ViewModelBase
{ 
    public SettingsScreenViewModel(IEnumerable<StatefulViewModelBase> statefulViewModels)
    {
        CommonSettingsViewModel = statefulViewModels.First<CommonSettingsViewModel>();
    }

    public CommonSettingsViewModel CommonSettingsViewModel { get; }
}