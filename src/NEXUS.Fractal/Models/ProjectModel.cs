using System;
using System.Collections.Generic;
using MessagePack;

namespace NEXUS.Fractal.Models;

[MessagePackObject]
public class ProjectModel
{
    [Key(nameof(Frames))]
    public List<FrameModel> Frames { get; set; } = [];

    [Key(nameof(Researches))]
    public List<ResearchModel> Researches { get; set; } = [];
}