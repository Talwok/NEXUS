using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Material.Icons;

namespace NEXUS.Growth.Views;

public partial class StartupSettingsElementView : UserControl
{
    public StartupSettingsElementView()
    {
        InitializeComponent();
        
        // if (Design.IsDesignMode)
        // {
        //     IconKind = MaterialIconKind.Abacus;
        //     Title = "Some title";
        //     Hint = "Some hint";
        //     IsCheckable = true;
        //     IsChecked = true;
        //     Content = new Button
        //     {
        //         Content = "Press me!"
        //     };
        // }
    }

    private string _title;

    public static readonly DirectProperty<StartupSettingsElementView, string> TitleProperty = AvaloniaProperty.RegisterDirect<StartupSettingsElementView, string>(
        nameof(Title), o => o.Title, (o, v) => o.Title = v);

    public string Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    private MaterialIconKind? _iconKind;

    public static readonly DirectProperty<StartupSettingsElementView, MaterialIconKind?> IconKindProperty = AvaloniaProperty.RegisterDirect<StartupSettingsElementView, MaterialIconKind?>(
        nameof(IconKind), o => o.IconKind, (o, v) => o.IconKind = v);

    public MaterialIconKind? IconKind
    {
        get => _iconKind;
        set => SetAndRaise(IconKindProperty, ref _iconKind, value);
    }

    private string? _hint;

    public static readonly DirectProperty<StartupSettingsElementView, string?> HintProperty = AvaloniaProperty.RegisterDirect<StartupSettingsElementView, string?>(
        nameof(Hint), o => o.Hint, (o, v) => o.Hint = v);

    public string? Hint
    {
        get => _hint;
        set => SetAndRaise(HintProperty, ref _hint, value);
    }

    private bool _isCheckable;

    public static readonly DirectProperty<StartupSettingsElementView, bool> IsCheckableProperty = AvaloniaProperty.RegisterDirect<StartupSettingsElementView, bool>(
        nameof(IsCheckable), o => o.IsCheckable, (o, v) => o.IsCheckable = v);

    public bool IsCheckable
    {
        get => _isCheckable;
        set => SetAndRaise(IsCheckableProperty, ref _isCheckable, value);
    }

    private bool? _isChecked;

    public static readonly DirectProperty<StartupSettingsElementView, bool?> IsCheckedProperty = AvaloniaProperty.RegisterDirect<StartupSettingsElementView, bool?>(
        nameof(IsChecked), o => o.IsChecked, (o, v) => o.IsChecked = v, false, BindingMode.TwoWay);

    public bool? IsChecked
    {
        get => _isChecked;
        set => SetAndRaise(IsCheckedProperty, ref _isChecked, value);
    }
}