using System.Collections.Generic;
using System.Linq;
using NEXUS.Growth.Models;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.ViewModels;

public class StartupViewModel : ViewModelBase
{
    public StartupViewModel()
    {
        Elements = ElementsHelper.GetElements().ToDictionary(item => $"{item.Name}, {item.Title}");
        
        SelectedPotential = Potentials.First();
        SelectedElement = Elements.First();
        SelectedSubstrateElement = Elements.First();
        SelectedProcess = Processes.First();
    }
    
    public Dictionary<Potential, string> Potentials => PotentialExtensions.GetDictionary();

    [Reactive] 
    public KeyValuePair<Potential, string> SelectedPotential { get; set; }
    
    public Dictionary<string, Element> Elements { get; }

    [Reactive]
    public KeyValuePair<string, Element>? SelectedElement { get; set; }
    
    public Dictionary<Process, string> Processes => ProcessExtensions.GetDictionary();

    [Reactive] 
    public KeyValuePair<Process, string> SelectedProcess { get; set; }
    
    [Reactive]
    public string? OutputFolder { get; set; }
    
    [Reactive] 
    public bool IsPaneOpened { get; set; }
    
    [Reactive]
    public double AtomCount { get; set; }

    [Reactive]
    public double TimeSteps { get; set; }

    [Reactive]
    public double BeamDiameter { get; set; }

    [Reactive]
    public double BeamEnergy { get; set; }

    [Reactive]
    public double BeamDelay { get; set; }

    [Reactive]
    public double EvolutionInitialDensity { get; set; }

    [Reactive] 
    public bool IsCubicConfiguration { get; set; } = true;

    [Reactive]
    public bool IsSphericConfiguration { get; set; }

    [Reactive] 
    public bool IsCenterPosition { get; set; } = true;

    [Reactive]
    public bool IsBottomPosition { get; set; }

    [Reactive]
    public double TemperatureInitial { get; set; }
    
    [Reactive]
    public double TemperatureEnd { get; set; }
    
    [Reactive]
    public double Cycles { get; set; }

    [Reactive]
    public bool TemperatureIntermediateEnable { get; set; }

    [Reactive]
    public double TemperatureIntermediatePercent { get; set; }

    [Reactive]
    public double TemperatureIntermediate { get; set; }

    [Reactive] 
    public bool IsOpenedType { get; set; } = true;
    
    [Reactive]
    public bool IsClosedType { get; set; }
    
    [Reactive]
    public bool IsPeriodicType { get; set; }
    
    [Reactive]
    public bool IsMotileBox { get; set; }

    [Reactive]
    public double BoxWidth { get; set; }
    
    [Reactive]
    public double BoxDepth { get; set; }
    
    [Reactive]
    public double BoxHeight { get; set; }

    [Reactive]
    public double SphereRadius { get; set; }

    [Reactive]
    public bool IsSubstrateNone { get; set; } = true;
    
    [Reactive]
    public bool IsSubstrateContinual { get; set; }
    
    [Reactive]
    public bool IsSubstrateDiscrete { get; set; }
    
    [Reactive] 
    public KeyValuePair<string, Element>? SelectedSubstrateElement { get; set; }

    [Reactive]
    public double Face { get; set; }

    [Reactive]
    public double AgileItemsHeight { get; set; }
    
    [Reactive]
    public double InitialAgileTemperature { get; set; }

    [Reactive]
    public bool IsCubicCell { get; set; }

    [Reactive] 
    public bool IsSphericCell { get; set; } = true;

    [Reactive] 
    public bool IsSavingVelocities { get; set; } = true;

    [Reactive]
    public bool IsAngularMomentumControl { get; set; }

    [Reactive]
    public double DumpCreationFrequency { get; set; }
    
    [Reactive]
    public double DumpSavingFrequency { get; set; }

    [Reactive]
    public double CnMax { get; set; }

    [Reactive]
    public double TimeVerlet { get; set; }
    
    [Reactive]
    public bool IsMaxwellCorrection { get; set; }
    
    [Reactive]
    public bool IsBerendsenThermostate { get; set; }

    [Reactive]
    public bool Is3dThermostate { get; set; }
    
    [Reactive]
    public double MaxwellCorrectionThreshold { get; set; }
    
    [Reactive]
    public double BerendsenThermostateParameter { get; set; }

    [Reactive]
    public bool IsAutoFolderNaming { get; set; }
}