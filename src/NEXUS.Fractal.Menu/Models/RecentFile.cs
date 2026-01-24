using System;
using System.Text.Json.Serialization;

namespace NEXUS.Fractal.Menu.Models;

public sealed class RecentFile
{
    public string FilePath { get; set; } = string.Empty;
    public DateTime LastOpened { get; set; }
    [JsonIgnore]
    public bool IsExisting { get; set; }
}
