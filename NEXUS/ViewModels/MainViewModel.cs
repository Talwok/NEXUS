using NEXUS.Helpers;

namespace NEXUS.ViewModels;

public class MainViewModel<TArgs> : ViewModelBase where TArgs : new()
{
    private TArgs? _args;
    
    public TArgs? Args => _args;

    public bool TrySetArgs(string? args) => 
        ArgumentParser.TryParse(args, out _args);
}