using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DynamicData;
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

    private void ToggleAllButton_OnClick(object? sender, RoutedEventArgs e) 
        => ImageTree.SelectAll();

    private void UntoggleAllButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            Images.SelectedItem = null;
            ImageTree.UnselectAll();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }
}