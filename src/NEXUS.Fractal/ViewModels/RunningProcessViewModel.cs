using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using NEXUS.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class RunningProcessViewModel
    : ViewModelBase
{
    private readonly IProgress<int>? _progress;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public RunningProcessViewModel(
        string description,
        Guid? id = null,
        CancellationTokenSource? cancellationTokenSource = null,
        int? maxProgress = null,
        bool isMarquee = false)
    {
        Description = description;

        Id = id ?? Guid.NewGuid();
        _cancellationTokenSource = cancellationTokenSource ?? new CancellationTokenSource();
        _progress = new Progress<int>(p => CurrentProgress = p);
        MaxProgress = maxProgress;
        IsMarquee = isMarquee;
        if (maxProgress == null)
        {
            IsMarquee = true;
        }
        CancelCommand = ReactiveCommand.Create(() => _cancellationTokenSource.Cancel());
    }

    public Guid Id { get; }
    public string Description { get; }
    public bool IsMarquee { get; }
    public int? MaxProgress { get; }

    [Reactive]
    public string ProgressText { get; set; }

    [Reactive]
    public int CurrentProgress { get; set; }

    public bool IsRunning => !_cancellationTokenSource.IsCancellationRequested;

    public CancellationTokenSource CancellationTokenSource => _cancellationTokenSource;
    public ICommand CancelCommand { get; }

    public void UpdateProgress(int progress)
    {
        _progress?.Report(progress);
        ProgressText = MaxProgress != null ? $"{progress}/{MaxProgress}" : $"{progress}/?";
    }

    public async Task IncrementProgressAsync()
        => await Task.Run(IncrementProgress);

    public void IncrementProgress()
    {
        UpdateProgress(CurrentProgress + 1);
    }
}