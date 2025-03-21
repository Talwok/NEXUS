using Microsoft.Extensions.DependencyInjection;
using NEXUS.ViewModels;

namespace NEXUS.Extensions;

public static partial class ServiceCollection
{
    public static Microsoft.Extensions.DependencyInjection.ServiceCollection AddCommon(this Microsoft.Extensions.DependencyInjection.ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<StatefulViewModelBase, CommonSettingsViewModel>();
        
        return serviceCollection;
    }
}