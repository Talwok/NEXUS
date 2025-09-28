using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Growth.Helpers;
using NEXUS.Growth.Models;
using NEXUS.Growth.Services;
using NEXUS.Parsers.CSEG;
using NEXUS.Parsers.CSEG.Models.Elements;
using NEXUS.Parsers.CSEG.Models.StartOptions;
using NEXUS.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.ViewModels;

public class StartupScreenViewModel : StatefulViewModelBase
{
    private bool _cellsChanging;

    private FilePickerFileType _startOptionsFileType = new("xml")
    {
        Patterns = new List<string>
        {
            "*.xml"
        }
    };


    [JsonConstructor]
    public StartupScreenViewModel() : base("StartupState.json")
    {
        Elements = ElementsHelper.GetElements().ToDictionary(item => $"{item.Global.Name}, {item.Global.Title}");
        Potentials = PotentialHelper.GetDictionary();
        Processes = ProcessHelper.GetDictionary();

        SelectedPotential = Potentials.First();
        SelectedElement = Elements.First();
        SelectedSubstrateElement = Elements.First();
        SelectedProcess = Processes.First();

        SearchOutputFolderCommand =
            ReactiveCommand.CreateFromTask(SearchOutputFolderAsync, outputScheduler: RxApp.MainThreadScheduler);
        SaveStartOptionsCommand =
            ReactiveCommand.CreateFromTask(SaveStartOptionsAsync, outputScheduler: RxApp.MainThreadScheduler);
        LoadStartOptionsCommand =
            ReactiveCommand.CreateFromTask(LoadStartOptionsAsync, outputScheduler: RxApp.MainThreadScheduler);
        StartProcessCommand = 
            ReactiveCommand.CreateRunInBackground(StartProcessAsync, outputScheduler: RxApp.MainThreadScheduler);

        PropertyChanged += OnPropertyChanged;
    }

    [ActivatorUtilitiesConstructor]
    public StartupScreenViewModel(SimulationService simulationSvc) : this()
    {
        SimulationService = simulationSvc;
    }
    
    
    public SimulationService SimulationService { get; set; }


    public Dictionary<Potential, string> Potentials { get; }

    [Reactive] public KeyValuePair<Potential, string> SelectedPotential { get; set; }

    [JsonIgnore] public Dictionary<string, ElementConfig> Elements { get; }

    [Reactive] public KeyValuePair<string, ElementConfig> SelectedElement { get; set; }

    public Dictionary<Process, string> Processes { get; }

    [Reactive] public KeyValuePair<Process, string> SelectedProcess { get; set; }

    [Reactive] public string? OutputFolder { get; set; }

    [Reactive] public double AtomCount { get; set; }

    [Reactive] public double TimeSteps { get; set; }

    [Reactive] public double BeamDiameter { get; set; }

    [Reactive] public double BeamEnergy { get; set; }

    [Reactive] public double BeamDelay { get; set; }

    [Reactive] public double EvolutionInitialDensity { get; set; }

    [Reactive] public bool IsCubicConfiguration { get; set; } = true;

    [Reactive] public bool IsSphericConfiguration { get; set; }

    [Reactive] public bool IsCenterPosition { get; set; } = true;

    [Reactive] public bool IsBottomPosition { get; set; }

    [Reactive] public double TemperatureInitial { get; set; }

    [Reactive] public double TemperatureEnd { get; set; }

    [Reactive] public double Cycles { get; set; }

    [Reactive] public bool TemperatureIntermediateEnable { get; set; }

    [Reactive] public double TemperatureIntermediatePercent { get; set; }

    [Reactive] public double TemperatureIntermediate { get; set; }

    [Reactive] public bool IsOpenedType { get; set; } = true;

    [Reactive] public bool IsClosedType { get; set; }

    [Reactive] public bool IsPeriodicType { get; set; }

    [Reactive] public bool IsMotileBox { get; set; }

    [Reactive] public double BoxWidth { get; set; }

    [Reactive] public double BoxDepth { get; set; }

    [Reactive] public double BoxHeight { get; set; }

    [Reactive] public double SphereRadius { get; set; }

    [Reactive] public bool IsSubstrateNone { get; set; } = true;

    [Reactive] public bool IsSubstrateContinual { get; set; }

    [Reactive] public bool IsSubstrateDiscrete { get; set; }

    [Reactive] public KeyValuePair<string, ElementConfig> SelectedSubstrateElement { get; set; }

    [Reactive] public double Face { get; set; }

    [Reactive] public double AgileItemsHeight { get; set; }

    [Reactive] public double InitialAgileTemperature { get; set; }

    [Reactive] public bool IsCubicCell { get; set; }

    [Reactive] public bool IsSphericCell { get; set; } = true;

    [Reactive] public bool IsSavingVelocities { get; set; } = true;

    [Reactive] public bool IsAngularMomentumControl { get; set; }

    [Reactive] public double DumpCreationFrequency { get; set; }

    [Reactive] public double DumpSavingFrequency { get; set; }

    [Reactive] public double CnMax { get; set; }

    [Reactive] public double TimeVerlet { get; set; }

    [Reactive] public bool IsMaxwellCorrection { get; set; }

    [Reactive] public bool IsBerendsenThermostate { get; set; }

    [Reactive] public bool Is3dThermostate { get; set; }

    [Reactive] public double MaxwellCorrectionThreshold { get; set; }

    [Reactive] public double BerendsenThermostateParameter { get; set; }

    [Reactive] public bool IsAutoFolderNaming { get; set; }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsAutoFolderNaming)
        {
            OutputFolder =
                $"[N{AtomCount}_dia{BeamDiameter}_E{BeamEnergy}_del{BeamDelay}_t{TemperatureInitial}]_TB_{SelectedElement.Value.Global.Title}_TB_{SelectedSubstrateElement.Value.Global.Title}";
        }

        if (!IsDeserializing)
            _ = Save(this);
    }

    private async Task StartProcessAsync()
    {
        if (string.IsNullOrEmpty(OutputFolder))
            return;
        
        Directory.CreateDirectory(OutputFolder);
            
        ElementConfigParser.Save(Path.Combine(OutputFolder, $"TB_{SelectedElement.Value.Global.Title}.conf"), SelectedElement.Value);
        ElementConfigParser.Save(Path.Combine(OutputFolder, $"TB_{SelectedSubstrateElement.Value.Global.Title}.conf"), SelectedSubstrateElement.Value);
        StartOptionsParser.Save(Path.Combine(OutputFolder, "start.xml"), GetStartOptions());

        await SimulationService.StartSimulation(OutputFolder);
    }

    private void SetAppConfiguration(AppConfiguration config)
    {
        IsAutoFolderNaming = config.IsStartupAutoFolderNaming;
    }

    private async Task LoadStartOptionsAsync()
    {
        var startOptionsFile = (await App.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Выберите файл для загрузки настроек",
                AllowMultiple = false,
                FileTypeFilter = [_startOptionsFileType],
            })).FirstOrDefault();

        if (startOptionsFile != null)
        {
            var startOptions = StartOptionsParser.Parse(startOptionsFile.Path.LocalPath);

            AtomCount = startOptions.StartOption.AtomCount;
            Cycles = startOptions.StartOption.Cycles;
            SelectedElement = Elements.First(elem =>
                elem.Value.Global.Title == startOptions.StartOption.Element.Replace("TB_", string.Empty));
            SelectedPotential = Potentials.First(pot =>
                pot.Key == PotentialHelper.GetPotential(startOptions.StartOption.Potential));
            SelectedProcess =
                Processes.First(proc => proc.Key == ProcessHelper.GetProcess(startOptions.StartOption.Process));
            TemperatureIntermediateEnable = startOptions.StartOption.Temperature2Enable;
            TemperatureEnd = startOptions.StartOption.TemperatureEnd;
            TemperatureInitial = startOptions.StartOption.TemperatureInitial;
            TimeSteps = startOptions.StartOption.TimeSteps;

            IsMotileBox = startOptions.StartOption.Box.Motile;

            switch (startOptions.StartOption.Box.Geometry)
            {
                case "sphere":
                    IsSphericCell = true;
                    IsCubicCell = false;
                    break;
                case "box":
                    IsCubicCell = true;
                    IsSphericCell = false;
                    break;
            }

            switch (startOptions.StartOption.Box.System)
            {
                case "closed":
                    IsClosedType = true;
                    break;
                case "opened":
                    IsOpenedType = true;
                    break;
                case "periodic":
                    IsPeriodicType = true;
                    break;
            }

            BoxHeight = startOptions.StartOption.Box.Height;
            BoxWidth = startOptions.StartOption.Box.Width;
            BoxDepth = startOptions.StartOption.Box.Depth;

            BeamEnergy = startOptions.StartOption.Epitaxy.Energy;
            BeamDelay = startOptions.StartOption.Epitaxy.Delay;
            BeamDiameter = startOptions.StartOption.Epitaxy.Diameter;

            SelectedSubstrateElement = Elements.First(elem =>
                elem.Value.Global.Title == startOptions.StartOption.Substrate.Element.Replace("TB_", string.Empty));
            AgileItemsHeight = startOptions.StartOption.Substrate.AgileHeight;
            InitialAgileTemperature = startOptions.StartOption.Substrate.AgileTemperature;
            Face = startOptions.StartOption.Substrate.Face;

            switch (startOptions.StartOption.Substrate.Type)
            {
                case "discrete":
                    IsSubstrateDiscrete = true;
                    break;
                case "continual":
                    IsSubstrateContinual = true;
                    break;
                case "none":
                    IsSubstrateNone = true;
                    break;
            }

            IsAngularMomentumControl = startOptions.StartOption.System.AngularMomentumControl;
            CnMax = startOptions.StartOption.System.CnMax;
            DumpCreationFrequency = startOptions.StartOption.System.DumpFrequency;
            DumpSavingFrequency = startOptions.StartOption.System.DumpSummaryFrequency;
            //startOptions.StartOption.System.OutputMethod;
            IsSavingVelocities = startOptions.StartOption.System.SaveVelocities;
            Is3dThermostate = startOptions.StartOption.System.Thermostate3dMode;
            IsBerendsenThermostate = startOptions.StartOption.System.ThermostateBerendsen;
            BerendsenThermostateParameter = startOptions.StartOption.System.ThermostateBerendsenParameter;
            IsMaxwellCorrection = startOptions.StartOption.System.ThermostateMaxwell;
            MaxwellCorrectionThreshold = startOptions.StartOption.System.ThermostateMaxwellThreshold;
            TimeVerlet = startOptions.StartOption.System.TimeVerlet;
        }
    }

    private async Task SaveStartOptionsAsync()
    {
        var startOptionsFile = await App.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Выберите папку и название файла для сохранения настроек",
            FileTypeChoices = new List<FilePickerFileType> { _startOptionsFileType },
            ShowOverwritePrompt = true
        });

        if (startOptionsFile != null)
        {
            StartOptionsParser.Save(startOptionsFile.Path.LocalPath, GetStartOptions());
        }
    }

    private StartOptions GetStartOptions()
    {
        var startOptions = new StartOptions
        {
            StartOption =
            {
                AtomCount = AtomCount,
                Cycles = Cycles,
                Element = $"TB_{SelectedElement.Value.Global.Title}",
                Potential = SelectedPotential.Key.ToOptionsString(),
                Process = SelectedProcess.Key.ToOptionsString(),
                Temperature2Enable = TemperatureIntermediateEnable,
                TemperatureEnd = TemperatureEnd,
                TemperatureInitial = TemperatureInitial,
                TimeSteps = TimeSteps,
                Box =
                {
                    Motile = IsMotileBox
                }
            }
        };

        if (IsCubicCell && !IsSphericCell)
        {
            startOptions.StartOption.Box.Geometry = "box";
        }
        else if (!IsCubicCell && IsSphericCell)
        {
            startOptions.StartOption.Box.Geometry = "sphere";
        }

        if (IsClosedType)
        {
            startOptions.StartOption.Box.System = "closed";
        }
        else if (IsOpenedType)
        {
            startOptions.StartOption.Box.System = "opened";
        }
        else
        {
            startOptions.StartOption.Box.System = "periodic";
        }

        startOptions.StartOption.Box.Height = BoxHeight;
        startOptions.StartOption.Box.Width = BoxWidth;
        startOptions.StartOption.Box.Depth = BoxDepth;

        startOptions.StartOption.Epitaxy.Energy = BeamEnergy;
        startOptions.StartOption.Epitaxy.Delay = BeamDelay;
        startOptions.StartOption.Epitaxy.Diameter = BeamDiameter;


        startOptions.StartOption.Substrate.Element = $"TB_{SelectedSubstrateElement.Value.Global.Title}";
        startOptions.StartOption.Substrate.AgileHeight = AgileItemsHeight;
        startOptions.StartOption.Substrate.AgileTemperature = InitialAgileTemperature;
        startOptions.StartOption.Substrate.Face = Face;

        if (IsSubstrateDiscrete)
        {
            startOptions.StartOption.Substrate.Type = "discrete";
        }
        else if (IsSubstrateContinual)
        {
            startOptions.StartOption.Substrate.Type = "continual";
        }
        else
        {
            startOptions.StartOption.Substrate.Type = "none";
        }

        startOptions.StartOption.System.AngularMomentumControl = IsAngularMomentumControl;
        startOptions.StartOption.System.CnMax = CnMax;
        startOptions.StartOption.System.DumpFrequency = DumpCreationFrequency;
        startOptions.StartOption.System.DumpSummaryFrequency = DumpSavingFrequency;
        startOptions.StartOption.System.SaveVelocities = IsSavingVelocities;
        startOptions.StartOption.System.Thermostate3dMode = Is3dThermostate;
        startOptions.StartOption.System.ThermostateBerendsen = IsBerendsenThermostate;
        startOptions.StartOption.System.ThermostateBerendsenParameter = BerendsenThermostateParameter;
        startOptions.StartOption.System.ThermostateMaxwell = IsMaxwellCorrection;
        startOptions.StartOption.System.ThermostateMaxwellThreshold = MaxwellCorrectionThreshold;
        startOptions.StartOption.System.TimeVerlet = TimeVerlet;
        startOptions.StartOption.System.OutputMethod = "standard";

        return startOptions;
    }

    private async Task SearchOutputFolderAsync()
    {
        var folders = await App.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Выберите папку для сохранения результатов",
            AllowMultiple = false
        });

        if (folders.Count >= 1)
        {
            OutputFolder = folders.First().Path.LocalPath;
        }
    }

    public ICommand SaveStartOptionsCommand { get; }

    public ICommand LoadStartOptionsCommand { get; }

    public ICommand StartProcessCommand { get; }

    public ICommand SearchOutputFolderCommand { get; }
}