using System.Collections;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace NEXUS.Fractal.Core.Behaviors;

public class SyncSelectedItemsBehavior : Behavior<ListBox>
{
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(SyncSelectedItemsBehavior), new PropertyMetadata(null, OnSelectedItemsChanged));

    public IList SelectedItems
    {
        get => (IList)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectionChanged += OnSelectionChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.SelectionChanged -= OnSelectionChanged;
        base.OnDetaching();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedItems != null)
        {
            SelectedItems.Clear();
            foreach (var item in AssociatedObject.SelectedItems)
                SelectedItems.Add(item);
        }
    }

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SyncSelectedItemsBehavior behavior && behavior.AssociatedObject != null)
        {
            behavior.AssociatedObject.SelectedItems.Clear();
            if (e.NewValue is IList newItems)
            {
                foreach (var item in newItems)
                    behavior.AssociatedObject.SelectedItems.Add(item);
            }
        }
    }
}