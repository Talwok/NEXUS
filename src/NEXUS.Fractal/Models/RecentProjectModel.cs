using System.IO;

namespace NEXUS.Fractal.Models;

public class RecentProjectModel
{
    public string Name { get; set; }
    public string? Directory { get; set; }
    public string FullPath { get; set; }
    public bool Exists => File.Exists(FullPath);
}