using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;
using NEXUS.Fractal.Controls.Surface;
using NEXUS.Fractal.Enums;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.Services;
using NEXUS.Fractal.ViewModels;
using NEXUS.Parsers.MDT.Models.Pallete;
using ReactiveUI;
using Ursa.Controls;

namespace NEXUS.Fractal.Views;

public partial class MainWindow : UrsaWindow
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

    private void TreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var fileTree = App.ServiceProvider.GetService<FileTreeViewModel>();
        var fileTabs = App.ServiceProvider.GetService<FileTabsViewModel>();
        if (fileTabs != null)
        {
            fileTabs.Tabs =
                new ObservableCollection<EntityNodeViewModel>(
                    fileTree?.SelectedNodes?.Cast<EntityNodeModel>().Select(node =>
                        new EntityNodeViewModel
                        {
                            Name = Path.GetFileName(node.FullPath),
                            FullPath = node.FullPath,
                            IsDirectory = Directory.Exists(node.FullPath),
                            Extension = Path.GetExtension(node.FullPath)
                        }) ?? []);

        }
    }
}