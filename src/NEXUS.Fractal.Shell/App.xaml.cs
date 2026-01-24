using System.Windows;
using NEXUS.Fractal.Core.Windows;
using NEXUS.Fractal.Explorer;
using NEXUS.Fractal.Menu;
using NEXUS.Fractal.Properties;
using NEXUS.Fractal.Shell.Views;
using NEXUS.Fractal.StatusLine;
using NEXUS.Fractal.Viewer;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace NEXUS.Fractal.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            IRegionManager regionManager = Container.Resolve<IRegionManager>();
            IEventAggregator eventAggregator = Container.Resolve<IEventAggregator>();
        }
        
        
        protected override void ConfigureModuleCatalog(IModuleCatalog catalog)
        {
            catalog.AddModule<ExplorerModule>();
            catalog.AddModule<ViewerModule>();
            catalog.AddModule<PropertiesModule>();
            catalog.AddModule<MenuModule>();
            catalog.AddModule<StatusLineModule>();
        }
        
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterWindows(containerRegistry);
            RegisterViewModels(containerRegistry);
            RegisterServices(containerRegistry);
        }

        private void RegisterWindows(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDialogWindow, CustomDialogWindow>();
        }

        private void RegisterViewModels(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterSingleton<MainWindowViewModel>();
        }
        
        private void RegisterServices(IContainerRegistry containerRegistry)
        {
            // containerRegistry.RegisterSingleton<ExplorerService>();
            // containerRegistry.RegisterSingleton<ViewerService>();
        }
    }
}