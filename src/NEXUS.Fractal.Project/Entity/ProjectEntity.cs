using ProtoBuf;

namespace NEXUS.Fractal.Project.Entity;

[ProtoContract]
public class ProjectEntity
{
    [ProtoMember(1)]
    public Guid Id { get; set; }
    [ProtoMember(2)]
    public ProjectEntityType Type { get; set; }
    [ProtoMember(3)]
    public string? Name { get; set; }
    [ProtoMember(4)]
    public DateTime LastModified { get; set; }
}