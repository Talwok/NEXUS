using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NEXUS.BaseClasses;
using NEXUS.Fractal.Core.Models.EventPayloads.Project;
using NEXUS.Fractal.Core.Services.Project;
using NEXUS.Fractal.Core.ViewModels.Project;
using Prism.Events;

namespace NEXUS.Fractal.Properties.ViewModels
{
    public partial class PropertiesViewModel : ObservableBaseObject
    {
        [ObservableProperty]
        private ProjectEntityViewModel? _selectedProjectEntity;
        
        public PropertiesViewModel(IEventAggregator eventAggregator, ProjectService projectService)
        {
            ProjectService = projectService;

            eventAggregator.GetEvent<PubSubEvent<SelectProjectEntityEventPayload>>()
                .Subscribe(SelectProjectEntity)
                .DisposeWith(Disposable);
        }
        
        public ProjectService ProjectService { get; }
        
        private void SelectProjectEntity(SelectProjectEntityEventPayload payload)
        {
            SelectedProjectEntity = payload.ProjectEntity;
        }
    }
}