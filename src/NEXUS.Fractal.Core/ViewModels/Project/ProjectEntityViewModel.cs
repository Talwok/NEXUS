using System.Drawing;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using NEXUS.BaseClasses;
using NEXUS.Fractal.Project.Entity;

namespace NEXUS.Fractal.Core.ViewModels.Project;

public partial class ProjectEntityViewModel : ObservableBaseObject
{
    [ObservableProperty]
    private Guid _id;
    
    [ObservableProperty]
    private ProjectEntityType _type;
    
    [ObservableProperty]
    private string? _name;
    
    [ObservableProperty]
    private DateTime _lastModified;

    [ObservableProperty]
    private ImageSource? _image;
    
    public ProjectEntityViewModel(ProjectEntity entity)
    {
        Id = entity.Id;
        Type = entity.Type;
        Name = entity.Name;
        LastModified = entity.LastModified;
    }

    public ProjectEntity ToEntity() => new()
    {
        Id = Id,
        Type = Type,
        Name = Name,
        LastModified = LastModified
    };
}