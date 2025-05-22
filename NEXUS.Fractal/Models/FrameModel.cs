using System;
using System.Collections.Generic;
using MessagePack;
using NEXUS.Fractal.Enums;

namespace NEXUS.Fractal.Models;

[MessagePackObject]
public class FrameModel
{
    [Key(nameof(Id))] public required Guid Id { get; set; }
    [Key(nameof(ParentId))] public Guid? ParentId { get; set; }
    [Key(nameof(SourceType))] public FrameSourceType SourceType { get; set; }
    [Key(nameof(Name))] public required string Name { get; set; }
    [Key(nameof(HeightMap))] public required float[,] HeightMap { get; set; }
    [Key(nameof(HeightSpacing))] public float HeightSpacing { get; set; }
    [Key(nameof(HeightScaling))] public float HeightScaling { get; set; }
    [Key(nameof(MetaData))] public required Dictionary<string, string> MetaData { get; set; }
    [Key(nameof(AmbientStrength))] public float AmbientStrength { get; set; } = 0.3f;
    [Key(nameof(SpecularStrength))] public float SpecularStrength { get; set; } = 0.4f;
    [Key(nameof(HeightMultiplier))] public float HeightMultiplier { get; set; } = 1;
    [Key(nameof(LightPositionX))] public float LightPositionX { get; set; } = 0;
    [Key(nameof(LightPositionY))] public float LightPositionY { get; set; } = 0;
    [Key(nameof(LightPositionZ))] public float LightPositionZ { get; set; } = 100;
    [Key(nameof(Zoom))] public float Zoom { get; set; } = 100;
    [Key(nameof(ModelYaw))] public float ModelYaw { get; set; }
    [Key(nameof(ModelPitch))] public float ModelPitch { get; set; } = 1.5f;
    [Key(nameof(ShowFoundation))] public bool ShowFoundation { get; set; } = true;
    [Key(nameof(ColorTableMaximum))] public float ColorTableMaximum { get; set; }
    [Key(nameof(ColorTableMinimum))] public float ColorTableMinimum { get; set; }
    [Key(nameof(ColorTableUpperSelection))] public float ColorTableUpperSelection { get; set; }
    [Key(nameof(ColorTableLowerSelection))] public float ColorTableLowerSelection { get; set; }
    
}