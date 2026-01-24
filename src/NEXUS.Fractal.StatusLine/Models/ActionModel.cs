using System;

namespace NEXUS.Fractal.StatusLine.Models;

public class ActionModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsMarquee { get; set; }
    public DateTime AddedDate { get; set; }
}