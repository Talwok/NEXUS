using Microsoft.Extensions.DependencyInjection;
using NEXUS.ViewModels;

namespace NEXUS.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static ServiceCollection AddCommon(this ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<StatefulViewModelBase, CommonSettingsViewModel>();
        
        return serviceCollection;
    }
}