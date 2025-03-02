using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using NEXUS.Growth.ViewModels;

namespace NEXUS.Growth.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (sender is NavigationView view && e.SelectedItem is NavigationViewItem { DataContext: ScreenMenuItem vm } item)
        {
            view.Content = vm.Screen;
        }
    }
}