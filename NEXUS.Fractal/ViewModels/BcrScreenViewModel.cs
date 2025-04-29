using System.Windows.Input;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Fractal.Services;
using NEXUS.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class BcrScreenViewModel : ViewModelBase
{
    public BcrScreenViewModel()
    {
        
    }

    [ActivatorUtilitiesConstructor]
    public BcrScreenViewModel(BcrService bcrService)
    {
        BcrService = bcrService;
        OpenCommand = ReactiveCommand.CreateFromTask(() => BcrService.OpenBcrAsync(), outputScheduler: AvaloniaScheduler.Instance);
    }

    [Reactive]
    public BcrService BcrService { get; set; }

    [Reactive]
    public bool IsPaneOpened { get; set; }
    
    public ICommand OpenCommand { get; }
}