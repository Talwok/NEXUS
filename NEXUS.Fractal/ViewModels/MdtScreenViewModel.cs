using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Fractal.Services;
using NEXUS.Parsers.MDT.Models.Frames;
using NEXUS.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class MdtScreenViewModel : ViewModelBase
{
    public MdtScreenViewModel()
    {
    }
 
    [ActivatorUtilitiesConstructor]
    public MdtScreenViewModel(MdtService mdtService)
    {
        MdtService = mdtService;
        OpenCommand = ReactiveCommand.CreateFromTask(() => MdtService.OpenMdtAsync(), outputScheduler: AvaloniaScheduler.Instance);
    }

    [Reactive]
    public MdtService MdtService { get; set; }

    [Reactive]
    public bool IsPaneOpened { get; set; }
    
    public ICommand OpenCommand { get; }
}