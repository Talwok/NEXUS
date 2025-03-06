using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NEXUS.Growth.Models;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.ViewModels;

public class StartupViewModel : StatefulViewModelBase
{
    public StartupViewModel() : base("StartupState.json")
    {
        
    }

    

}