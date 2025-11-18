using System.Collections.Generic;

namespace NEXUS.Fractal.Models;

public class EntityNodeModel
{
    public string FullPath { get; set; }
    public List<EntityNodeModel> Children { get; set; } = [];
}