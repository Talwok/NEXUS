using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using NEXUS.BaseClasses;
using NEXUS.Fractal.Core.Models.EventPayloads;
using NEXUS.Fractal.Explorer.Models;
using NEXUS.Fractal.Explorer.Services;
using Prism.Commands;
using Prism.Events;

namespace NEXUS.Fractal.Explorer.ViewModels;

public partial class ExplorerViewModel : ObservableBaseObject
{
    private readonly ExplorerService _explorerService;
        
    private readonly PubSubEvent<SelectFolderEventPayload> _selectFolderEvent;
    
    [ObservableProperty]
    private ExplorerTypeRecord _selectedExplorerTypePair;

    [ObservableProperty]
    private string _searchText;

    [ObservableProperty]
    private bool _isInitialized;
    
    public ExplorerViewModel(ExplorerService explorerService, IEventAggregator eventAggregator)
    {
        _explorerService = explorerService;
        
        _selectFolderEvent = eventAggregator.GetEvent<PubSubEvent<SelectFolderEventPayload>>();
        
        eventAggregator
            .GetEvent<PubSubEvent<OnFolderSelectedEventPayload>>()
            .Subscribe(OnFolderSelected)
            .DisposeWith(Disposable);
        
        IsInitialized = false;
        
        ExplorerTypes =
        [
            new ExplorerTypeRecord("Проект", ExplorerType.Project),
            new ExplorerTypeRecord("Файлы", ExplorerType.Files)
        ];
            
        SelectedExplorerTypePair = ExplorerTypes.First();

        SelectFolderCommand = new DelegateCommand(SelectFolder);
    }

    private async void OnFolderSelected(OnFolderSelectedEventPayload obj)
    {
        try
        {
            await _explorerService.InitializeByFolder(obj.FolderPath);
            IsInitialized = true;
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    private void SelectFolder()
    {
        _selectFolderEvent.Publish(new SelectFolderEventPayload());
    }

    public ICommand SelectFolderCommand { get; set; } 
        
    public IEnumerable<ExplorerTypeRecord> ExplorerTypes { get; }
        
}