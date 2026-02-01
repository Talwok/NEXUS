using NEXUS.Fractal.Core.ViewModels.Project;

namespace NEXUS.Fractal.Core.Models.EventPayloads.Project;

public record SelectProjectEntityEventPayload(ProjectEntityViewModel ProjectEntity, IEnumerable<ProjectEntityViewModel> ProjectEntities);