using System;
using FluentAvalonia.UI.Controls;
using Material.Icons;
using NEXUS.ViewModels;

namespace NEXUS.Fractal.ViewModels;

public class InfoMessageViewModel : ViewModelBase
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public MaterialIconKind Icon { get; set; } = MaterialIconKind.Information;
    public bool IsOpen { get; set; } = true;
    public InfoBarSeverity Severity { get; set; } = InfoBarSeverity.Informational;
}