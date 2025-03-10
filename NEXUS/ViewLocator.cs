using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using NEXUS.ViewModels;

namespace NEXUS;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;
        
        var name = data.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Assembly.GetEntryAssembly()?.GetType(name)
            ?? Assembly.GetExecutingAssembly().GetType(name);
        
        if (type != null)
        {
            var control = (Control)Activator.CreateInstance(type)!;
            control.DataContext = data;
            return control;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}