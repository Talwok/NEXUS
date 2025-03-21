using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using NEXUS.Fractal.ViewModels;

namespace NEXUS.Fractal.Views;

public partial class ImagesScreenView : UserControl
{
    public ImagesScreenView()
    {
        InitializeComponent();
    }

    private void TabViewItem_OnCloseRequested(TabViewItem sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (DataContext is ImagesScreenViewModel vm)
        {
            vm.SelectedImages.Remove((ImageFileViewModel)args.Item);
        }
    }
}