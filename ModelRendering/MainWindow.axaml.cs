using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering;

namespace ModelRendering;

public partial class MainWindow : Window
{
   public MainWindow()
   {
      InitializeComponent();
      this.AttachDevTools();
      RendererDiagnostics.DebugOverlays = RendererDebugOverlays.Fps;
   }

   private void InitializeComponent()
   {
      AvaloniaXamlLoader.Load(this);
   }
}