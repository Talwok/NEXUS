using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering;
using Silk.NET.OpenGL;

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

   protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
   {
      base.OnAttachedToVisualTree(e);
      
   }

   

   private void Button_OnClick(object? sender, RoutedEventArgs e)
   {
     
   }
}