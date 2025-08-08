using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Extensions;
using NEXUS.Fractal.Services;
using NEXUS.Fractal.ViewModels;

namespace NEXUS.Fractal.Views;

public partial class Inspector : UserControl
{
    public Inspector()
    {
        InitializeComponent();
    }

    private void CollapseFrameTree(object? sender, RoutedEventArgs e)
    {
        var containers = FrameTree.GetRealizedTreeContainers();
        foreach (var container in containers)
        {
            if(container is TreeViewItem item)
                FrameTree.CollapseSubTree(item);
        }
    }

    private void ExpandFrameTree(object? sender, RoutedEventArgs e)
    {
        var containers = FrameTree.GetRealizedTreeContainers();
        foreach (var container in containers)
        {
            if(container is TreeViewItem item)
                FrameTree.ExpandSubTree(item);
        }
    }

    private void ExpandFrame(object? sender, RoutedEventArgs e)
    {
        void Expand(FrameViewModel frame)
        {
            frame.IsExpanded = true;
            foreach (var child in frame.Children)
            {
                Expand(child);
            }
        }
        
        if (sender is Control { DataContext: FrameViewModel frameViewModel })
        {
            Expand(frameViewModel);
        }
    }

    private void CollapseFrame(object? sender, RoutedEventArgs e)
    {
        void Collapse(FrameViewModel frame)
        {
            frame.IsExpanded = false;
            foreach (var child in frame.Children)
            {
                Collapse(child);
            }
        }
        
        if (sender is Control { DataContext: FrameViewModel frameViewModel })
        {
            Collapse(frameViewModel);
        }
    }
}