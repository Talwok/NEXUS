using NEXUS.Fractal.Properties.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace NEXUS.Fractal.Properties
{
    public class PropertiesModule : IModule
    {
        public static string RegionName = "PropertiesRegion";
        
        
        public void OnInitialized(IContainerProvider containerProvider)
        {
            IRegionManager regionManager = containerProvider.Resolve<IRegionManager>();
            RegisterRegions(regionManager);
        }
        
        private void RegisterRegions(IRegionManager regionManager)
        {
            regionManager.RegisterViewWithRegion<PropertiesView>(RegionName);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterViewModels(containerRegistry);
            RegisterServices(containerRegistry);
        }

        private void RegisterViewModels(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterSingleton<MainWindowViewModel>();
        }
        
        private void RegisterServices(IContainerRegistry containerRegistry)
        {
            // containerRegistry.RegisterSingleton<ExplorerService>();
        }
    }
}