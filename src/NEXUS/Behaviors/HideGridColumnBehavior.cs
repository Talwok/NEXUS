using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Xaml.Interactivity;

namespace NEXUS.Behaviors;

public class HideGridColumnBehavior : Behavior<Grid>
{
    // Определяем свойство для номера столбца
    public static readonly StyledProperty<int> ColumnIndexProperty =
        AvaloniaProperty.Register<HideGridColumnBehavior, int>(
            nameof(ColumnIndex), 
            defaultBindingMode: BindingMode.TwoWay);

    public int ColumnIndex
    {
        get => GetValue(ColumnIndexProperty);
        set => SetValue(ColumnIndexProperty, value);
    }

    // Определяем свойство для видимости
    public static readonly StyledProperty<bool> IsVisibleProperty =
        AvaloniaProperty.Register<HideGridColumnBehavior, bool>(
            nameof(IsVisible), 
            true,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsVisible
    {
        get => GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    // Присоединенное свойство для хранения исходной ширины
    private static readonly AttachedProperty<GridLength> OriginalWidthProperty =
        AvaloniaProperty.RegisterAttached<HideGridColumnBehavior, Control, GridLength>(
            "OriginalWidth");

    protected override void OnAttached()
    {
        base.OnAttached();
        
        // Подписываемся на изменения свойств
        this.GetObservable(ColumnIndexProperty).Subscribe(_ => UpdateColumnVisibility());
        this.GetObservable(IsVisibleProperty).Subscribe(_ => UpdateColumnVisibility());
        
        // Первоначальное обновление
        UpdateColumnVisibility();
    }

    private void UpdateColumnVisibility()
    {
        if (AssociatedObject == null || 
            ColumnIndex < 0 || 
            ColumnIndex >= AssociatedObject.ColumnDefinitions.Count)
            return;

        var columnDefinition = AssociatedObject.ColumnDefinitions[ColumnIndex];
        
        if (IsVisible)
        {
            // Восстанавливаем исходную ширину, если она была сохранена
            if (columnDefinition.GetValue(OriginalWidthProperty) is GridLength originalWidth)
            {
                columnDefinition.Width = originalWidth;
                columnDefinition.ClearValue(OriginalWidthProperty);
            }
        }
        else
        {
            // Сохраняем исходную ширину, если еще не сохранили
            if (!columnDefinition.IsSet(OriginalWidthProperty))
            {
                columnDefinition.SetValue(OriginalWidthProperty, columnDefinition.Width);
            }
            
            // Устанавливаем нулевую ширину
            columnDefinition.Width = new GridLength(0);
        }
    }
}