using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using NEXUS.Fractal.Extensions;
using NEXUS.Fractal.Models;
using NEXUS.Fractal.Services;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class ProjectViewModel : ViewModelBase
{
    private readonly ProjectService _svc;

    public ProjectViewModel(string projectPath, ProjectModel project)
    {
        Name = Path.GetFileNameWithoutExtension(projectPath);
        Directory = Path.GetDirectoryName(projectPath);
        
        Frames = new ObservableCollection<FrameViewModel>(project.Frames.BuildTree());
        Researches = new ObservableCollection<ResearchModel>(project.Researches);
    }
    
    [Reactive]
    public string Name { get; set; }
    
    [Reactive]
    public string? Directory { get; set; }
    
    public ObservableCollection<FrameViewModel> Frames { get; set; }
    public ObservableCollection<ResearchModel> Researches { get; set; }

    public ProjectModel GetModel()
    {
        return new ProjectModel
        {
            Frames = Frames.Select(f => f.GetModel()).ToList(),
            Researches = Researches.ToList()
        };
    }
}
