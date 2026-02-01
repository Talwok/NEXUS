using System;
using System.Text.Json.Serialization;

namespace NEXUS.Fractal.Menu.Models;

public sealed class RecentFolder
{
    public string FolderPath { get; set; } = string.Empty;
    public DateTime LastOpened { get; set; }
    [JsonIgnore]
    public bool IsExisting { get; set; }
}
