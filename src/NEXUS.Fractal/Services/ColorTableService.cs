using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using NEXUS.Converters;
using NEXUS.Parsers.MDT;
using NEXUS.Parsers.MDT.Models.Pallete;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.Services;

public class ColorTableService : StatefulServiceBase
{
    public static string FileName = "ColorTableService.json";
    public ColorTableService() : base(FileName)
    {
        foreach (var newTable in PaletteParser.GetStandardPalleteFiles().SelectMany(file => file.Tables))
        {
            if (!string.Equals(newTable.Title, "Unnamed Palette", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(newTable.Title, "My Palette", StringComparison.OrdinalIgnoreCase))
                ColorTables.Add(newTable);
        }

        if (ColorTables.FirstOrDefault() is { } table)
            SelectedColorTable = table;
    }

    [JsonIgnore]
    public ObservableCollection<PaletteColorTable> ColorTables { get; } = [];

    [Reactive, JsonIgnore]
    public PaletteColorTable? SelectedColorTable { get; set; }
}