using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Material.Icons;

namespace NEXUS.Styles.Control.Custom;

public partial class CompositionElement : UserControl
{
    public CompositionElement()
    {
        InitializeComponent();
    }

    private string? _title;

    public static readonly DirectProperty<CompositionElement, string?> TitleProperty = AvaloniaProperty.RegisterDirect<CompositionElement, string?>(
        nameof(Title), o => o.Title, (o, v) => o.Title = v);

    public string? Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    private MaterialIconKind? _iconKind;

    public static readonly DirectProperty<CompositionElement, MaterialIconKind?> IconKindProperty = AvaloniaProperty.RegisterDirect<CompositionElement, MaterialIconKind?>(
        nameof(IconKind), o => o.IconKind, (o, v) => o.IconKind = v);

    public MaterialIconKind? IconKind
    {
        get => _iconKind;
        set => SetAndRaise(IconKindProperty, ref _iconKind, value);
    }

    private string? _hint;

    public static readonly DirectProperty<CompositionElement, string?> HintProperty = AvaloniaProperty.RegisterDirect<CompositionElement, string?>(
        nameof(Hint), o => o.Hint, (o, v) => o.Hint = v);

    public string? Hint
    {
        get => _hint;
        set => SetAndRaise(HintProperty, ref _hint, value);
    }

    private bool _isCheckable;

    public static readonly DirectProperty<CompositionElement, bool> IsCheckableProperty = AvaloniaProperty.RegisterDirect<CompositionElement, bool>(
        nameof(IsCheckable), o => o.IsCheckable, (o, v) => o.IsCheckable = v);

    public bool IsCheckable
    {
        get => _isCheckable;
        set => SetAndRaise(IsCheckableProperty, ref _isCheckable, value);
    }

    private bool _isChecked;

    public static readonly DirectProperty<CompositionElement, bool> IsCheckedProperty = AvaloniaProperty.RegisterDirect<CompositionElement, bool>(
        nameof(IsChecked), o => o.IsChecked, (o, v) => o.IsChecked = v, false, BindingMode.TwoWay);

    public bool IsChecked
    {
        get => _isChecked;
        set => SetAndRaise(IsCheckedProperty, ref _isChecked, value);
    }
}