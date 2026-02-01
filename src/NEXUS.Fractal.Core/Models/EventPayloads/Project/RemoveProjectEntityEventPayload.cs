namespace NEXUS.Fractal.Core.Models.EventPayloads.Project;

public record RemoveProjectEntityEventPayload(IEnumerable<Guid> Ids);