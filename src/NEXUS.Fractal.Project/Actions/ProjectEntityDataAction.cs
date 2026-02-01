using ProtoBuf;

namespace NEXUS.Fractal.Project.Actions;

[ProtoContract]
public class ProjectEntityDataAction
{
    [ProtoMember(1)]
    public Guid Id { get; set; }
    [ProtoMember(2)]
    public Guid EntityId { get; set; }
    [ProtoMember(3)]
    public Guid ResultId { get; set; }
    [ProtoMember(4)]
    public ProjectEntityDataActionType Type { get; set; }
    [ProtoMember(5)]
    public DateTime LastModified { get; set; }
}