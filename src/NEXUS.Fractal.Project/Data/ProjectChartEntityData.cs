using ProtoBuf;

namespace NEXUS.Fractal.Project.Data;

[ProtoContract]
public class ProjectChartEntityData
{
    [ProtoMember(1)]
    public float[]? X { get; set; }
    [ProtoMember(2)]
    public float[]? Y { get; set; }
    [ProtoMember(3)]
    public string? XAxisLabel { get; set; }
    [ProtoMember(4)]
    public string? YAxisLabel { get; set; }
    [ProtoMember(5)]
    public string? XUnitsLabel { get; set; }
    [ProtoMember(6)]
    public string? YUnitsLabel { get; set; }
}