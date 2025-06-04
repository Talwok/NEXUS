using System;
using MessagePack;

namespace NEXUS.Fractal.Models;

[MessagePackObject]
public class ResearchModel
{
    [Key(nameof(ParentId))] public Guid ParentId { get; set; }
    [Key(nameof(Name))] public string? Name { get; set; }
}