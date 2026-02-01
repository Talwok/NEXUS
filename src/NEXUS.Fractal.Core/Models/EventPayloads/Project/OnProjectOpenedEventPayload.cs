namespace NEXUS.Fractal.Core.Models.EventPayloads.Project;

public record OnProjectOpenedEventPayload(string ProjectPath, DateTime OpenedAt);
