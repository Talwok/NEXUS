using System.Collections.Generic;
using NEXUS.Extensions;
using NEXUS.ViewModels;

namespace NEXUS.Fractal.ViewModels;

public class SettingsScreenViewModel(IEnumerable<StatefulViewModelBase> statefulViewModels) : ViewModelBase
{
    public CommonSettingsViewModel CommonSettings { get; } = statefulViewModels.First<CommonSettingsViewModel>();
}