using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Growth.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.ViewModels;

public class StartupScreenViewModel : ScreenViewModelBase
{
    private bool _cellsChanging;

    private FilePickerFileType _startOptionsFileType = new("xml")
    {
        Patterns = new List<string>
        {
            "*.xml"
        }
    };

    private readonly AppConfiguration _configuration;

    public StartupScreenViewModel()
    {
        if (Application.Current is App app)
        {
            Startup = app.ServiceProvider.GetRequiredService<StartupViewModel>();
        }

        this.PropertyChanged += OnPropertyChanged;

        SearchOutputFolderCommand = ReactiveCommand.CreateFromTask(SearchOutputFolderAsync);
        SaveStartOptionsCommand = ReactiveCommand.CreateFromTask(SaveStartOptionsAsync); 
        LoadStartOptionsCommand = ReactiveCommand.CreateFromTask(LoadStartOptionsAsync);
        StartProcessCommand = ReactiveCommand.Create(StartProcess);
        SetStartOptions(StartOptionsExtensions.DeserializeDefaultsFromXml());

        _configuration = AppConfigurationExtensions.DeserializeDefaultsFromXml() ?? new AppConfiguration();

        SetAppConfiguration(_configuration);
    }

    [Reactive]
    public StartupViewModel Startup { get; set; }

    private void StartProcess()
    {
        if (Startup.OutputFolder != null)
        {
            var outputFolder = $"..\\{Startup.OutputFolder}";
            var elementsFolder = "Assets\\Elements";
            var elementFileName = Startup.SelectedElement?.Value.FileName;
            var substrateElementFileName = Startup.SelectedSubstrateElement?.Value.FileName;

            if(Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);
            
            Directory.CreateDirectory(outputFolder);
            
            using var stream = File.Open(Path.Combine(outputFolder, "start.xml"), FileMode.OpenOrCreate);
            
            GetStartOptions().SerializeToXml(stream);
            
            _configuration.RecentFolders.Add(Startup.OutputFolder);
            _configuration.SerializeDefaultsToXml();

            if (elementFileName != null && !File.Exists(Path.Combine(outputFolder, $"{elementFileName}.conf")))
            {
                File.Copy(Path.Combine(elementsFolder, $"{elementFileName}.conf"),
                    Path.Combine(outputFolder, $"{elementFileName}.conf"));
            }

            if (substrateElementFileName != null && !File.Exists(Path.Combine(outputFolder, $"{substrateElementFileName}.conf")))
            {
                File.Copy(Path.Combine(elementsFolder, $"{substrateElementFileName}.conf"),
                    Path.Combine(outputFolder, $"{substrateElementFileName}.conf"));
            }
            
            SetAppConfiguration(_configuration);
        }
    }

    private AppConfiguration GetAppConfiguration()
    {
        _configuration.IsStartupAutoFolderNaming = Startup.IsAutoFolderNaming;
        return _configuration;
    }

    private void SetAppConfiguration(AppConfiguration config)
    {
        Startup.IsAutoFolderNaming = config.IsStartupAutoFolderNaming;
    }
    
    private void SetStartOptions(StartOptions? options)
    {
        if (options?.Options != null)
        {
            var option = options.Options.FirstOrDefault();
            
            if (option != null)
            {
                Startup.SelectedProcess = Startup.Processes.First(p => p.Key.ToOptionsString() == option.Process);
                Startup.SelectedPotential = Startup.Potentials.First(p => p.Key.ToOptionsString() == option.Potential);
                Startup.SelectedElement = Startup.Elements.First(e => e.Value.FileName == option.Element);
                Startup.AtomCount = option.AtomCount;
                Startup.TimeSteps = option.TimeSteps;
                Startup.TemperatureInitial = option.TemperatureInitial;
                Startup.TemperatureIntermediateEnable = option.TemperatureIntermediateEnable;
                Startup.TemperatureIntermediate = option.TemperatureIntermediate;
                Startup.TemperatureIntermediatePercent = option.TemperatureIntermediatePercent;
                Startup.TemperatureEnd = option.TemperatureEnd;
                Startup.Cycles = option.Cycles;
            }
            
            if (option?.Epitaxy != null)
            {
                Startup.BeamDiameter = option.Epitaxy.Diameter;
                Startup.BeamEnergy = option.Epitaxy.Energy;
                Startup.BeamDelay = option.Epitaxy.Delay;
            }

            if (option?.Evolution != null)
            {
                Startup.EvolutionInitialDensity = option.Evolution.InitialDensity;
                Startup.IsCubicConfiguration = option.Evolution.InitialConfiguration == "cube";
                Startup.IsSphericConfiguration = option.Evolution.InitialConfiguration == "sphere";
                Startup.IsCenterPosition = option.Evolution.InitialPosition == "centered";
                Startup.IsBottomPosition = option.Evolution.InitialPosition == "bottom";
            }
            
            if (option?.Substrate != null)
            {
                Startup.AgileItemsHeight = option.Substrate.AgileHeight;
                Startup.InitialAgileTemperature = option.Substrate.AgileTemperature;
                Startup.Face = option.Substrate.Face;
                Startup.IsSubstrateNone = option.Substrate.Type == "none";
                Startup.IsSubstrateContinual = option.Substrate.Type == "continual";
                Startup.IsSubstrateDiscrete = option.Substrate.Type == "discrete";
                Startup.SelectedSubstrateElement = Startup.Elements.First(e => e.Value.FileName == option.Substrate.Element);
            }

            if (option?.Box != null)
            {
                Startup.IsMotileBox = option.Box.Motile;
                Startup.BoxWidth = option.Box.Width;
                Startup.BoxDepth = option.Box.Depth;
                Startup.BoxHeight = option.Box.Height;
                Startup.SphereRadius = option.Box.Radius;
                Startup.IsClosedType = option.Box.System == "closed";
                Startup.IsOpenedType = option.Box.System == "opened";
                Startup.IsPeriodicType = option.Box.System == "periodic";
                Startup.IsCubicCell = option.Box.Geometry == "cube";
                Startup.IsSphericCell = option.Box.Geometry == "sphere";
            }

            if (option?.System != null)
            {
                Startup.CnMax = option.System.CnMax;
                Startup.TimeVerlet = option.System.TimeVerlet;
                Startup.IsSavingVelocities = option.System.SaveVelocities;
                Startup.IsAngularMomentumControl = option.System.AngularMomentumControl;
                Startup.DumpCreationFrequency = option.System.DumpFrequency;
                Startup.DumpSavingFrequency = option.System.DumpSummaryFrequency;
                Startup.IsMaxwellCorrection = option.System.ThermostateMaxwell;
                Startup.MaxwellCorrectionThreshold = option.System.ThermostateMaxwellThreshold;
                Startup.IsBerendsenThermostate = option.System.ThermostateBerendsen;
                Startup.BerendsenThermostateParameter = option.System.ThermostateBerendsenParameter;
                Startup.Is3dThermostate = option.System.Thermostate3DMode;
            }
        }
    }
    
    private StartOptions GetStartOptions()
    {
        var epitaxy = new Epitaxy
        {
            Diameter = Startup.BeamDiameter,
            Energy = Startup.BeamEnergy,
            Delay = Startup.BeamDelay
        };

        var evolution = new Evolution
        {
            InitialDensity = Startup.EvolutionInitialDensity,
            InitialConfiguration = Startup.IsCubicConfiguration ? "cube" : "sphere",
            InitialPosition = Startup.IsCenterPosition ? "centered" : "bottom"
        };

        var substrate = new Substrate
        {
            Element = Startup.SelectedSubstrateElement?.Value.FileName,
            AgileHeight = Startup.AgileItemsHeight,
            AgileTemperature = Startup.InitialAgileTemperature,
            Face = Startup.Face
        };

        if (Startup.IsSubstrateNone)
        {
            substrate.Type = "none";
        }
        else if (Startup.IsSubstrateContinual)
        {
            substrate.Type = "continual";
        }
        else if (Startup.IsSubstrateDiscrete)
        {
            substrate.Type = "discrete";
        }

        var box = new Box
        {
            Motile = Startup.IsMotileBox,
            Width = Startup.BoxWidth,
            Depth = Startup.BoxDepth,
            Height = Startup.BoxHeight,
            Radius = Startup.SphereRadius
        };
        
        if (Startup.IsClosedType)
        {
            box.System = "closed";
        }
        else if (Startup.IsOpenedType)
        {
            box.System = "opened";
        }
        else if (Startup.IsPeriodicType)
        {
            box.System = "periodic";
        }
        
        if (Startup.IsCubicCell)
        {
            box.Geometry = "cube";
        }
        else if (Startup.IsSphericCell)
        {
            box.Geometry = "sphere";
        }

        var system = new SystemSettings
        {
            CnMax = Startup.CnMax,
            TimeVerlet = Startup.TimeVerlet,
            SaveVelocities = Startup.IsSavingVelocities,
            AngularMomentumControl = Startup.IsAngularMomentumControl,
            DumpFrequency = Startup.DumpCreationFrequency,
            DumpSummaryFrequency = Startup.DumpSavingFrequency,
            OutputMethod = "standard",
            ThermostateMaxwell = Startup.IsMaxwellCorrection,
            ThermostateMaxwellThreshold = Startup.MaxwellCorrectionThreshold,
            ThermostateBerendsen = Startup.IsBerendsenThermostate,
            ThermostateBerendsenParameter = Startup.BerendsenThermostateParameter,
            Thermostate3DMode = Startup.Is3dThermostate
        };
        
        var option = new StartOption
        {
            Process = Startup.SelectedProcess.Key.ToOptionsString(),
            Potential = Startup.SelectedPotential.Key.ToOptionsString(),
            Element = Startup.SelectedElement?.Value.FileName,
            AtomCount = Startup.AtomCount,
            TimeSteps = Startup.TimeSteps,
            TemperatureInitial = Startup.TemperatureInitial,
            TemperatureIntermediateEnable = Startup.TemperatureIntermediateEnable,
            TemperatureIntermediate = Startup.TemperatureIntermediate,
            TemperatureEnd = Startup.TemperatureEnd,
            Cycles = Startup.Cycles,
            Epitaxy = epitaxy,
            Evolution = evolution,
            Substrate = substrate,
            Box = box,
            System = system
        };
        
        var options = new StartOptions { Options = [ option ] };
        
        return options;
    }
    
    private async Task LoadStartOptionsAsync()
    {
        if (Application.Current is App app)
        {
            var startOptionsFile = (await app.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Выберите файл для загрузки настроек", 
                    AllowMultiple = false, 
                    FileTypeFilter = [_startOptionsFileType],
                })).FirstOrDefault();

            if (startOptionsFile != null)
            {
                SetStartOptions(StartOptionsExtensions.DeserializeFromXml(await startOptionsFile.OpenReadAsync()));
            }
        }
    }

    private async Task SaveStartOptionsAsync()
    {
        if (Application.Current is App app)
        {
            var startOptionsFile = await app.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Выберите папку и название файла для сохранения настроек",
                FileTypeChoices = new List<FilePickerFileType> { _startOptionsFileType },
                ShowOverwritePrompt = true
            });
            
            if (startOptionsFile != null)
            {
                var options = GetStartOptions();
                options.SerializeToXml(await startOptionsFile.OpenWriteAsync());
            }
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var configNames = new string[]
        {
            nameof(Startup.IsPaneOpened),
            nameof(Startup.IsAutoFolderNaming),   
        };

        if (configNames.Contains(e.PropertyName))
        {
            GetAppConfiguration().SerializeDefaultsToXml();
        }
        
        if (e.PropertyName == nameof(Startup.IsCubicCell) && !_cellsChanging)
        {
            _cellsChanging = true;
            Startup.IsSphericCell = !Startup.IsCubicCell;
            _cellsChanging = false;
        }
        else if (e.PropertyName == nameof(Startup.IsSphericCell) && !_cellsChanging)
        {
            _cellsChanging = true;
            Startup.IsCubicCell = !Startup.IsSphericCell;
            _cellsChanging = false;
        }
        else
        {
            
        }
    }

    private async Task SearchOutputFolderAsync()
    {
        if (Application.Current is App app)
        {
            var folders = await app.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Выберите папку для сохранения результатов",
                AllowMultiple = false
            });

            if (folders.Count >= 1)
            {
                Startup.OutputFolder = folders.First().Path.LocalPath;
            }
        }
    }

    public ICommand SaveStartOptionsCommand { get; }
    
    public ICommand LoadStartOptionsCommand { get; }
    
    public ICommand StartProcessCommand { get; }
    
    public ICommand SearchOutputFolderCommand { get; }

}