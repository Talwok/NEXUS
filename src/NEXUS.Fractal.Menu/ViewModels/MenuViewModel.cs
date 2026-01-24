using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using NEXUS.BaseClasses;
using NEXUS.Fractal.Core.Models.EventPayloads;
using Prism.Events;

namespace NEXUS.Fractal.Menu.ViewModels
{
    public class MenuViewModel : ObservableBaseObject
    {
        private readonly PubSubEvent<OnFolderSelectedEventPayload> _onFolderSelectedEvent;

        public MenuViewModel(IEventAggregator eventAggregator)
        {
            _onFolderSelectedEvent = eventAggregator.GetEvent<PubSubEvent<OnFolderSelectedEventPayload>>();
            
            eventAggregator
                .GetEvent<PubSubEvent<SelectFolderEventPayload>>()
                .Subscribe(OnSelectFolderEvent)
                .DisposeWith(Disposable);
            
            SelectFolderCommand = new DelegateCommand(SelectFolder);
        }
        
        public ICommand SelectFolderCommand { get; }
        
        private void OnSelectFolderEvent(SelectFolderEventPayload obj) 
            => SelectFolder();

        private void SelectFolder()
        {
            var openFolderDialog = new OpenFolderDialog
            {
                AddToRecent = true,
                Title = "Выберите рабочую папку",
            };

            if (openFolderDialog.ShowDialog() is true)
            {
                _onFolderSelectedEvent.Publish(new OnFolderSelectedEventPayload(openFolderDialog.FolderName));
            }
        }
    }
}