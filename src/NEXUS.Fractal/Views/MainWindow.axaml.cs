using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using NEXUS.Fractal.ViewModels;
using NEXUS.ViewModels;

namespace NEXUS.Fractal.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        if (DataContext is MainWindowViewModel vm
            && sender is Grid grid
            && vm.SettingsMenuItemScreen.Settings is { } settings
            && grid.ColumnDefinitions.Count == settings.ColumnWidths.Count)
        {
            for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
            {
                var width = grid.ColumnDefinitions[i].ActualWidth;
                if (Math.Abs(settings.ColumnWidths[i] - width) > double.Epsilon)
                    settings.ColumnWidths[i] = width;
            }
        }
    }
}