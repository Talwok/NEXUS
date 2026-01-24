using Prism.Services.Dialogs;
using System.Windows;
using System.Windows.Media;
using Window = HandyControl.Controls.Window;

namespace NEXUS.Fractal.Core.Windows;

public class CustomDialogWindow : Window, IDialogWindow
{
    public CustomDialogWindow()
    {
        Width = 400;
        Height = 300;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.SingleBorderWindow;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = (Brush)FindResource("RegionBrush");
        Foreground = (Brush)FindResource("TextIconBrush");
    }

    public IDialogResult Result { get; set; } = new DialogResult(ButtonResult.None);
}