using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.ReactiveUI;
using NEXUS.Fractal.Services;
using NEXUS.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Fractal.Models;

namespace NEXUS.Fractal.ViewModels;

public class ImagesScreenViewModel : ViewModelBase
{
    public ImagesScreenViewModel()
    {
        
    }
 
    [ActivatorUtilitiesConstructor]
    public ImagesScreenViewModel(ImageService imageService, CalculationService calculationService)
    {
        ImageService = imageService;
        CalculationService = calculationService;
        
        SelectFolderCommand = ReactiveCommand.CreateRunInBackground(ImageService.InitFolder, outputScheduler: AvaloniaScheduler.Instance);
        AddImagesCommand = ReactiveCommand.CreateFromTask(ImageService.AddImages, outputScheduler: AvaloniaScheduler.Instance);
        RemoveImagesCommand = ReactiveCommand.Create<IEnumerable<ImageFileViewModel>>(ImageService.RemoveImages, outputScheduler: AvaloniaScheduler.Instance);
        RemoveImageCommand = ReactiveCommand.Create<ImageFileViewModel>((image) => File.Delete(image.Path), outputScheduler: AvaloniaScheduler.Instance);
        BoxCountingCommand = ReactiveCommand.CreateRunInBackground(() => CalculationService.Run(Calculation.BoxCounting), outputScheduler: AvaloniaScheduler.Instance);
        TriangulationCommand = ReactiveCommand.CreateRunInBackground(() => CalculationService.Run(Calculation.Triangulation), outputScheduler: AvaloniaScheduler.Instance);
        
        ApplyFilterCommand = ReactiveCommand.CreateFromTask<MatrixType>(ImageService.ApplyFilter, outputScheduler: AvaloniaScheduler.Instance);

        ExpandAllCommand = ReactiveCommand.Create(() =>
        {
            for (var i = 0; i < ImageService.TreeImages.Count; i++) 
                imageService.TreeImages[i].IsExpanded = true;
        }, outputScheduler: AvaloniaScheduler.Instance);
        CollapseAllCommand = ReactiveCommand.Create(() =>
        {
            for (var i = 0; i < ImageService.TreeImages.Count; i++) 
                imageService.TreeImages[i].IsExpanded = false;
        }, outputScheduler: AvaloniaScheduler.Instance);
        
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
    
    [Reactive]
    public IEnumerable<MatrixType> MatrixTypes { get; set; } = Enum.GetValues<MatrixType>();
    
    public ICommand SelectFolderCommand { get; }
    public ICommand AddImagesCommand { get; }
    public ICommand RemoveImagesCommand { get; }
    public ICommand RemoveImageCommand { get; }
    public ICommand BoxCountingCommand { get; }
    public ICommand TriangulationCommand { get; }
    
    public ICommand ApplyFilterCommand { get; }
    public ICommand CollapseAllCommand { get; }
    public ICommand ExpandAllCommand { get; }
}