using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using NEXUS.Growth.ViewModels;
using NEXUS.ViewModels;

namespace NEXUS.Growth.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (sender is NavigationView view && e.SelectedItem is NavigationViewItem { DataContext: ScreenMenuItem vm })
        {
            view.Content = vm.Screen;
        }
    }
}