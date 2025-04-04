using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using NEXUS.ViewModels;

namespace NEXUS.Fractal.Views;

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