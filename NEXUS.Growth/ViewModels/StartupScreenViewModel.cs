using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using NEXUS.Growth.Models;
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
    

    public StartupScreenViewModel() : base("StartupState.json") 
    {
        Elements = ElementsHelper.GetElements().ToDictionary(item => $"{item.Name}, {item.Title}");
        Potentials = PotentialExtensions.GetDictionary();
        Processes = ProcessExtensions.GetDictionary();
        
        SelectedPotential = Potentials.First();
        SelectedElement = Elements.First();
        SelectedSubstrateElement = Elements.First();
        SelectedProcess = Processes.First();
        
        SearchOutputFolderCommand = ReactiveCommand.CreateFromTask(SearchOutputFolderAsync);
        SaveStartOptionsCommand = ReactiveCommand.CreateFromTask(SaveStartOptionsAsync); 
        LoadStartOptionsCommand = ReactiveCommand.CreateFromTask(LoadStartOptionsAsync);
        //StartProcessCommand = ReactiveCommand.Create(StartProcess);

        this.PropertyChanged += OnPropertyChanged;
    }
    
    
    public Dictionary<Potential, string> Potentials { get; }

    [Reactive] 
    public KeyValuePair<Potential, string> SelectedPotential { get; set; }
    
    public Dictionary<string, Element> Elements { get; }

    [Reactive]
    public KeyValuePair<string, Element>? SelectedElement { get; set; }

    public Dictionary<Process, string> Processes { get; }

    [Reactive] 
    public KeyValuePair<Process, string> SelectedProcess { get; set; }
    
    [Reactive]
    public string? OutputFolder { get; set; }
    
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
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(!IsDeserializing)
            _ = Save(this);
    }
    
    /*private void StartProcess()
    {
        if (OutputFolder != null)
        {
            var outputFolder = $"..\\{OutputFolder}";
            var elementsFolder = "Assets\\Elements";
            var elementFileName = SelectedElement?.Value.FileName;
            var substrateElementFileName = SelectedSubstrateElement?.Value.FileName;

            if(Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);
            
            Directory.CreateDirectory(outputFolder);
            
            using var stream = File.Open(Path.Combine(outputFolder, "start.xml"), FileMode.OpenOrCreate);
            
            GetStartOptions().SerializeToXml(stream);
            
            _configuration.RecentFolders.Add(OutputFolder);
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
        _configuration.IsStartupAutoFolderNaming = IsAutoFolderNaming;
        return _configuration;
    }*/

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
           
        }
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