using NEXUS.Fractal.Explorer.Services;
using NEXUS.Fractal.Explorer.ViewModels;
using NEXUS.Fractal.Explorer.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace NEXUS.Fractal.Explorer
{
    public class ExplorerModule : IModule
    {
        public const string RegionName = "ExplorerRegion";

        public void OnInitialized(IContainerProvider containerProvider)
        {
            IRegionManager regionManager = containerProvider.Resolve<IRegionManager>();
            RegisterRegions(regionManager);
        }
        
        private void RegisterRegions(IRegionManager regionManager)
        {
            regionManager.RegisterViewWithRegion<ExplorerView>(RegionName);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterDialogs(containerRegistry);
            RegisterViewModels(containerRegistry);
            RegisterServices(containerRegistry);
        }

        private void RegisterDialogs(IContainerRegistry containerRegistry)
        {

            containerRegistry.RegisterDialog<EnormousEntriesDialogView, EnormousEntriesDialogViewModel>();
            //containerRegistry.RegisterDialog<EnormousEntriesDialogView, EnormousEntriesDialogViewModel>();
        }

        private void RegisterViewModels(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterSingleton<MainWindowViewModel>();
        }
        
        private void RegisterServices(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ExplorerService>();
        }
    }
}