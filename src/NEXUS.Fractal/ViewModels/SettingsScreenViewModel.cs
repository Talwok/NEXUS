using System.Collections.Generic;
using System.Collections.ObjectModel;
using NEXUS.Extensions;
using NEXUS.ViewModels;

namespace NEXUS.Fractal.ViewModels;

public class SettingsScreenViewModel(IEnumerable<StatefulViewModelBase> statefulViewModels) : ViewModelBase
{
    public CommonSettingsViewModel CommonSettings { get; } = statefulViewModels.First<CommonSettingsViewModel>();
    public SettingsViewModel Settings { get; } = statefulViewModels.First<SettingsViewModel>();
}