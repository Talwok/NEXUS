using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using NEXUS.Growth.Models;
using NEXUS.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.ViewModels;

public class ViewerScreenViewModel : StatefulViewModelBase
{
    private readonly AppConfiguration _configuration;

    public ViewerScreenViewModel() : base("ViewerState.json")
    {
        _configuration = AppConfigurationExtensions.DeserializeDefaultsFromXml() ?? new AppConfiguration();

        Processes = ProcessExtensions.GetDictionary();
        
        // LastProcesses = new ObservableCollection<ProcessDirectoryViewModel>(_configuration.RecentFolders
        //     .Where(folder => File.Exists($"..\\{folder}\\start.xml"))
        //     .Select(folder =>
        //     {
        //         var options = StartOptionsExtensions.DeserializeFromXml($"..\\{folder}\\start.xml");
        //         var dirInfo = new DirectoryInfo($"..\\{folder}");
        //         var processDirectory = new ProcessDirectoryViewModel
        //         {
        //             Process = Processes.First(p => p.Key.ToOptionsString() == options?.Options?[0].Process).Value,
        //             ElementName = options?.Options?[0].Element,
        //             SubstrateElementName = options?.Options?[0].Substrate?.Element,
        //             AtomCount = options?.Options?[0].AtomCount ?? 0,
        //             Date = dirInfo.CreationTime.ToShortDateString(),
        //             Folder = folder
        //         };
        //         return processDirectory;
        //     }));
    }

    public Dictionary<Process, string> Processes { get; }

    [Reactive]
    public ObservableCollection<ProcessDirectoryViewModel> LastProcesses { get; set; }

    [Reactive]
    public bool IsPaneOpened { get; set; }
}