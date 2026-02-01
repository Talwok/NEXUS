using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using NEXUS.BaseClasses;
using NEXUS.Fractal.Core.Models.EventPayloads.Project;
using NEXUS.Fractal.Core.Services.Project;
using NEXUS.Fractal.Core.ViewModels.Project;
using NEXUS.Fractal.Explorer.Models;
using Prism.Commands;
using Prism.Events;

namespace NEXUS.Fractal.Explorer.ViewModels;

public partial class ExplorerViewModel : ObservableBaseObject
{
    private readonly PubSubEvent<OpenProjectEventPayload> _openProjectEvent;
    private readonly PubSubEvent<CreateProjectEventPayload> _createProjectEvent;
    private readonly PubSubEvent<SelectProjectEntityEventPayload> _selectionEvent;
    private readonly PubSubEvent<ImportProjectEntityEventPayload> _importProjectEntityEvent;
    private readonly PubSubEvent<RemoveProjectEntityEventPayload> _removeProjectEntityEvent;
    
    [ObservableProperty]
    private ExplorerTypeRecord _selectedExplorerTypePair;

    [ObservableProperty]
    private string _searchText;

    [ObservableProperty]
    private bool _isInitialized;

    [ObservableProperty]
    private ProjectEntityViewModel? _selectedEntity;


    public ExplorerViewModel(IEventAggregator eventAggregator, ProjectService projectService)
    {
        ProjectService = projectService;
        
        _openProjectEvent = eventAggregator.GetEvent<PubSubEvent<OpenProjectEventPayload>>();
        _createProjectEvent = eventAggregator.GetEvent<PubSubEvent<CreateProjectEventPayload>>();
        _selectionEvent = eventAggregator.GetEvent<PubSubEvent<SelectProjectEntityEventPayload>>();
        _importProjectEntityEvent = eventAggregator.GetEvent<PubSubEvent<ImportProjectEntityEventPayload>>();
        _removeProjectEntityEvent = eventAggregator.GetEvent<PubSubEvent<RemoveProjectEntityEventPayload>>();
        
        IsInitialized = false;
        
        eventAggregator.GetEvent<PubSubEvent<OnProjectOpenedEventPayload>>()
            .Subscribe(_ => IsInitialized = true)
            .DisposeWith(Disposable);
        
        OpenProjectCommand = new DelegateCommand(OpenProject);
        CreateProjectCommand = new DelegateCommand(CreateProject);
        ImportCommand = new DelegateCommand(Import);
        RemoveSelectedCommand = new DelegateCommand(RemoveSelected);
        SelectionChangedCommand = new DelegateCommand<SelectionChangedEventArgs>(OnSelectionChanged);
    }

    public ProjectService ProjectService { get; }
    
    public ICommand OpenProjectCommand { get; } 
    public ICommand CreateProjectCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand RemoveSelectedCommand { get;  }
    
    public ICommand SelectionChangedCommand { get; }
    
    
    private void OpenProject()
    {
        IsInitialized = false;
        _openProjectEvent.Publish(new OpenProjectEventPayload());
    }

    private void CreateProject()
    {
        IsInitialized = false;
        _createProjectEvent.Publish(new CreateProjectEventPayload());
    }

    private void Import() => 
        _importProjectEntityEvent.Publish(new ImportProjectEntityEventPayload());

    private void OnSelectionChanged(SelectionChangedEventArgs args)
    {
        if (SelectedEntity == null) 
            return;
        
        var alreadySelectedIds = ProjectService.SelectedEntities.Select(vm => vm.Id).ToList();
        List<ProjectEntityViewModel> innerAddList = [];
        foreach (var item in args.AddedItems.Cast<ProjectEntityViewModel>())
        {
            if (!alreadySelectedIds.Contains(item.Id))
            {
                innerAddList.Add(item);
            }
        } 
        
        ProjectService.SelectedEntities.AddRange(innerAddList);
        
        foreach (var item in args.RemovedItems.Cast<ProjectEntityViewModel>())
        {
            if (alreadySelectedIds.Contains(item.Id))
            {
                ProjectService.SelectedEntities.Remove(item);
            }
        } 
        
        _selectionEvent.Publish(new SelectProjectEntityEventPayload(SelectedEntity, ProjectService.SelectedEntities));
    }
    
    private void RemoveSelected()
    {
        if (SelectedEntity == null) 
            return;
        _removeProjectEntityEvent.Publish(new RemoveProjectEntityEventPayload(ProjectService.SelectedEntities.Select(ent => ent.Id).ToList()));
    }
}