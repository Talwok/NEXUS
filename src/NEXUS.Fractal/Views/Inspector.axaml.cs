using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
}