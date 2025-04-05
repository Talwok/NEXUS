using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using NEXUS.Fractal.ViewModels;

namespace NEXUS.Fractal.Views;

public partial class MdtScreenView : UserControl
{
    public MdtScreenView()
    {
        InitializeComponent();
    }

    private void TabViewItem_OnCloseRequested(TabViewItem sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (DataContext is MdtScreenViewModel vm)
        {
            //vm.SelectedImages.Remove((MdtFrameViewModel)args.Item);
        }
    }

    private void Images_OnSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (sender is TabView tabView)
        {

        }
    }
}