using System;
using System.Collections.ObjectModel;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class ImageFileViewModel : ViewModelBase
{
    [Reactive]
    public string? Path { get; init; }
    [Reactive]
    public string? Name { get; init; }
    [Reactive]
    public string? Extension { get; init; }
    [Reactive] 
    public Guid? Id { get; init; }
    [Reactive] 
    public bool IsExpanded { get; set; } = true;
    public ObservableCollection<ImageFileViewModel> Children { get; } = [];
}