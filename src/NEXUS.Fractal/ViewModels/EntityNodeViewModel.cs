using System.Collections.ObjectModel;
using NEXUS.Converters;
using NEXUS.Fractal.Services;
using NEXUS.Parsers.MDT.Models.Pallete;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;

namespace NEXUS.Fractal.ViewModels;

public class EntityNodeViewModel : ReactiveObject
{
    private readonly ColorTableService _svc;

    public EntityNodeViewModel()
    {
        if (App.ServiceProvider
                .GetServices<StatefulServiceBase>()
                .FirstOrDefault<ColorTableService>() is { } svc)
        {
            _svc = svc;

            _svc.WhenAnyValue(s => s.SelectedColorTable)
                .Subscribe(colorTable => ColorTable = colorTable);
        }

        this.WhenAnyValue(
                vm => vm.ColorTableMinimum,
                vm => vm.ColorTableMaximum,
                vm => vm.ColorTableLowerSelection,
                vm => vm.ColorTableUpperSelection,
                vm => vm.ColorTable)
            .Subscribe(range =>
            {
                var (min, max, lower, upper, table) = range;
                ColorTableRange = new(max, min, upper, lower, table.Colors);
            });

        if (ColorTableMaximum == 0 && ColorTableMinimum == 0)
        {
            var (min, max) = (0f, 100f);
            ColorTableMaximum = ColorTableUpperSelection = max;
            ColorTableMinimum = ColorTableLowerSelection = min;
        }
    }

    [Reactive]
    public PaletteColorTable? ColorTable { get; set; }

    [Reactive]
    public string Name { get; set; }

    [Reactive]
    public bool IsDirectory { get; set; }

    [Reactive]
    public ObservableCollection<EntityNodeViewModel> Children { get; set; } = [];

    [Reactive]
    public string FullPath { get; set; }

    [Reactive]
    public bool IsExpanded { get; set; }

    [Reactive]
    public string Extension { get; set; }

    [Reactive]
    public float ColorTableMaximum { get; set; }

    [Reactive]
    public float ColorTableMinimum { get; set; }

    [Reactive]
    public float ColorTableUpperSelection { get; set; }

    [Reactive]
    public float ColorTableLowerSelection { get; set; }

    [Reactive]
    public ColorTableRange ColorTableRange { get; set; }
}