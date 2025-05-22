using NEXUS.Fractal.Models;
using NEXUS.ViewModels;

namespace NEXUS.Fractal.ViewModels;

public abstract class ResearchViewModel : ViewModelBase
{
    public abstract ResearchModel GetModel();
}