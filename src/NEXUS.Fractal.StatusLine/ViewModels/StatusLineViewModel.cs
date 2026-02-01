#nullable enable
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using NEXUS.BaseClasses;
using NEXUS.Fractal.Core.Models.EventPayloads.MarqueeAction;
using NEXUS.Fractal.Core.Models.EventPayloads.Project;
using NEXUS.Fractal.StatusLine.Models;
using Prism.Events;

namespace NEXUS.Fractal.StatusLine.ViewModels
{
    public partial class StatusLineViewModel : ObservableBaseObject
    {
        private SourceCache<ActionModel, Guid> _actionsSource;
        
        [ObservableProperty]
        private ReadOnlyObservableCollection<ActionModel> _actions;

        [ObservableProperty]
        private ActionModel? _lastAction;

        [ObservableProperty]
        private bool _hasActions;

        [ObservableProperty]
        private string _projectName;
        
        public StatusLineViewModel(IEventAggregator eventAggregator)
        {
            _actionsSource = new SourceCache<ActionModel, Guid>(action => action.Id);
            
            _actionsSource.Connect()
                .Bind(out var actions)
                .Subscribe()
                .DisposeWith(Disposable);
            
            Actions = actions;
            
            eventAggregator
                .GetEvent<PubSubEvent<MarqueeActionStartEventPayload>>()
                .Subscribe(OnMarqueeActionStart)
                .DisposeWith(Disposable);
            
            eventAggregator
                .GetEvent<PubSubEvent<MarqueeActionEndEventPayload>>()
                .Subscribe(OnMarqueeActionEnd)
                .DisposeWith(Disposable);
            
            eventAggregator
                .GetEvent<PubSubEvent<OnProjectOpenedEventPayload>>()
                .Subscribe(OnProjectOpened)
                .DisposeWith(Disposable);
        }

        private void OnProjectOpened(OnProjectOpenedEventPayload payload)
        {
            ProjectName = Path.GetFileNameWithoutExtension(payload.ProjectPath);
        }

        private void OnMarqueeActionStart(MarqueeActionStartEventPayload payload)
        {
            var action = new ActionModel
            {
                Id = payload.Id,
                Name = payload.Name,
                Description = payload.Description,
                IsMarquee = true,
                AddedDate = DateTime.Now
            };
            
            _actionsSource.AddOrUpdate(action);
            
            LastAction = action;
            
            HasActions = Actions.Count > 0;
        }

        private void OnMarqueeActionEnd(MarqueeActionEndEventPayload obj)
        {
            _actionsSource.Remove(obj.ActionId);

            if (obj.ActionId == LastAction?.Id)
            {
                LastAction = Actions
                    .OrderBy(item => Math.Abs((item.AddedDate - DateTime.Now).Ticks))
                    .FirstOrDefault();
            }
            
            HasActions = Actions.Count > 0;
        }
    }
}