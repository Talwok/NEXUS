using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.ReactiveUI;
using NEXUS.Fractal.Services;
using NEXUS.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using NEXUS.Fractal.Models;

namespace NEXUS.Fractal.ViewModels;

public class ImagesScreenViewModel : ViewModelBase
{
    public ImagesScreenViewModel(ImageService imageService, CalculationService calculationService)
    {
        ImageService = imageService;
        CalculationService = calculationService;
        
        SelectFolderCommand = ReactiveCommand.CreateRunInBackground(ImageService.InitFolder, outputScheduler: AvaloniaScheduler.Instance);
        AddImagesCommand = ReactiveCommand.CreateFromTask(ImageService.AddImages, outputScheduler: AvaloniaScheduler.Instance);
        RemoveImagesCommand = ReactiveCommand.Create<IEnumerable<ImageFileViewModel>>(ImageService.RemoveImages, outputScheduler: AvaloniaScheduler.Instance);

        BoxCountingCommand = ReactiveCommand.CreateRunInBackground(() => CalculationService.Run(Calculation.BoxCounting), outputScheduler: AvaloniaScheduler.Instance);
        TriangulationCommand = ReactiveCommand.CreateRunInBackground(() => CalculationService.Run(Calculation.Triangulation), outputScheduler: AvaloniaScheduler.Instance);
        
        this.WhenAnyValue(vm => vm.SelectedImages)
            .Subscribe(images =>
            {
                ImageService.SelectedImages = images;
            });
    }


    [Reactive]
    public bool IsPaneOpened { get; set; }
    
    public ImageService ImageService { get; }
    public CalculationService CalculationService { get; }

    [Reactive] 
    public ObservableCollection<ImageFileViewModel> SelectedImages { get; set; } = [];
    
    public ICommand SelectFolderCommand { get; }
    public ICommand AddImagesCommand { get; }
    public ICommand RemoveImagesCommand { get; }
    
    public ICommand BoxCountingCommand { get; }
    public ICommand TriangulationCommand { get; }
}