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

namespace NEXUS.Fractal.ViewModels;

public class ImagesScreenViewModel : ViewModelBase
{
    public ImagesScreenViewModel(ImageService imageService)
    {
        ImageService = imageService;
        SelectFolderCommand = ReactiveCommand.CreateFromTask(ImageService.InitFolder, outputScheduler: AvaloniaScheduler.Instance);
        AddImagesCommand = ReactiveCommand.CreateFromTask(ImageService.AddImages, outputScheduler: AvaloniaScheduler.Instance);
        RemoveImagesCommand = ReactiveCommand.Create<IEnumerable<ImageFileViewModel>>(ImageService.RemoveImages, outputScheduler: AvaloniaScheduler.Instance);

        this.WhenAnyValue(vm => vm.SelectedImages)
            .Subscribe(images =>
            {
                ImageService.SelectedImages = images.Select(img => img.Path);
            });
    }

    [Reactive]
    public bool IsPaneOpened { get; set; }
    
    public ImageService ImageService { get; }

    [Reactive] 
    public ObservableCollection<ImageFileViewModel> SelectedImages { get; set; } = [];
    
    public ICommand SelectFolderCommand { get; }
    public ICommand AddImagesCommand { get; }
    public ICommand RemoveImagesCommand { get; }
}