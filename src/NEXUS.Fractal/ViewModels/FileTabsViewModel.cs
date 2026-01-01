using NEXUS.ViewModels;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using DynamicData;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.Services;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class FileTabsViewModel : ViewModelBase
{
    private readonly FileWatcherService _service;
    private EntityNodeViewModel? _lastSelectedTab;

    public FileTabsViewModel(FileWatcherService service)
    {
        _service = service;

        this.WhenAnyValue(vm => vm.SelectedTab)
            .Subscribe(OnTabChanged);

        RemoveTabCommand = ReactiveCommand.Create<EntityNodeViewModel>(tab => Tabs?.Remove(tab));
    }

    public ICommand RemoveTabCommand { get; }

    [Reactive]
    public EntityNodeViewModel? SelectedTab { get; set; }

    [Reactive]
    public ObservableCollection<string> Breadcrumbs { get; set; } = [];

    [Reactive]
    public ObservableCollection<EntityNodeViewModel> Tabs { get; set; }

    private void OnTabChanged(EntityNodeViewModel? tab)
    {
        if (tab == null) return;

        _lastSelectedTab?.Clear();
        _lastSelectedTab = tab;

        var pathLasting = tab.FullPath.Split(_service.WatchedFolder + "\\").LastOrDefault();
        if (pathLasting != null)
        {
            Breadcrumbs.Clear();
            Breadcrumbs.AddRange(pathLasting.Split('\\'));
        }

        _ = tab.LoadData();
    }
}