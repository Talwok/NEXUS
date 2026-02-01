using System.Windows;
using Prism.Commands;
using System.Windows.Input;
using NEXUS.BaseClasses;
using NEXUS.Fractal.Core.Models.EventPayloads.Project;
using NEXUS.Fractal.Menu.Services;
using Prism.Events;

namespace NEXUS.Fractal.Menu.ViewModels
{
    public class MenuViewModel : ObservableBaseObject
    {
        private readonly PubSubEvent<OpenProjectEventPayload> _openProjectEvent;
        private readonly PubSubEvent<CreateProjectEventPayload> _createProjectEvent;
        private readonly PubSubEvent<ImportProjectEntityEventPayload> _importProjectEntityEvent;
        private readonly PubSubEvent<SaveProjectEventPayload> _saveProjectEvent;

        public MenuViewModel(IEventAggregator eventAggregator, RecentProjectsService recentProjectsService)
        {
            RecentProjectsService = recentProjectsService;
            
            _openProjectEvent = eventAggregator.GetEvent<PubSubEvent<OpenProjectEventPayload>>();
            _createProjectEvent = eventAggregator.GetEvent<PubSubEvent<CreateProjectEventPayload>>();
            _importProjectEntityEvent = eventAggregator.GetEvent<PubSubEvent<ImportProjectEntityEventPayload>>();
            _saveProjectEvent = eventAggregator.GetEvent<PubSubEvent<SaveProjectEventPayload>>();
            
            CreateProjectCommand = new DelegateCommand(CreateProject);
            OpenProjectCommand = new DelegateCommand(OpenProject);
            OpenRecentProjectCommand = new DelegateCommand<string>(OpenProject);
            SaveProjectCommand = new DelegateCommand(SaveProject);
            ImportCommand = new DelegateCommand(Import);
            CloseAppCommand = new DelegateCommand(CloseApp);
        }
        
        public ICommand CreateProjectCommand { get; }
        public ICommand OpenProjectCommand { get; }
        public ICommand OpenRecentProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        
        public ICommand ImportCommand { get; }
        public ICommand CloseAppCommand { get; }
        
        public RecentProjectsService RecentProjectsService { get; }
        
        private void OpenProject() => OpenProject(null);
        
        private void OpenProject(string? initialPath) =>
            _openProjectEvent.Publish(new OpenProjectEventPayload(initialPath));

        private void CreateProject() => 
            _createProjectEvent.Publish(new CreateProjectEventPayload());
        
        private void Import() => 
            _importProjectEntityEvent.Publish(new ImportProjectEntityEventPayload());
        
        private void SaveProject() => 
            _saveProjectEvent.Publish(new SaveProjectEventPayload());

        private void CloseApp() => Application.Current.Shutdown();
        
    }
}