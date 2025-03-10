using System.Text.Json.Serialization;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Fractal.ViewModels;

public class ImagesScreenViewModel : StatefulViewModelBase
{
    public ImagesScreenViewModel() : base("ImagesState.json")
    {
        
    }
    
    [Reactive, JsonIgnore]
    public int TotalImages { get; set; }
}