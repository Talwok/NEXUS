using NEXUS.Fractal.StatusLine.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace NEXUS.Fractal.StatusLine
{
    public class StatusLineModule : IModule
    {
        public static string RegionName = "StatusLineRegion";
        
        public void OnInitialized(IContainerProvider containerProvider)
        {
            IRegionManager regionManager = containerProvider.Resolve<IRegionManager>();
            RegisterRegions(regionManager);
        }
        
        private void RegisterRegions(IRegionManager regionManager)
        {
            regionManager.RegisterViewWithRegion<StatusLineView>(RegionName);
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