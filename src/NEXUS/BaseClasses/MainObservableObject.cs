using NEXUS.Parsers;

namespace NEXUS.BaseClasses;

public class MainObservableObject<TArgs> : ObservableBaseObject where TArgs : new()
{
    private TArgs? _args;
    public TArgs? Args => _args;
    public bool TrySetArgs(string? args) => ArgumentParser.TryParse(args, out _args);
}