

using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using NEXUS.BaseClasses;
using NEXUS.Parsers;
using NEXUS.Parsers.Mdt.Models.Pallete;

namespace NEXUS.Fractal.Core.Services.Project;

public partial class ColorTableService : ObservableBaseObject
{
    [ObservableProperty]
    private ObservableCollection<PaletteColorTable> _colorTables;
    
    [ObservableProperty]
    private PaletteColorTable? _selectedColorTable;
    
    public ColorTableService()
    {
        ColorTables = [];
        
        foreach (var newTable in PaletteParser.GetStandardPalleteFiles().SelectMany(file => file.Tables))
        {
            if (!string.Equals(newTable.Title, "Unnamed Palette", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(newTable.Title, "My Palette", StringComparison.OrdinalIgnoreCase))
                ColorTables.Add(newTable);
        }

        if (ColorTables.FirstOrDefault() is { } table)
            SelectedColorTable = table;
    }
}