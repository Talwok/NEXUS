using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NEXUS.Growth.Services;



//Ещё нужно предусмотреть закрытие всех процессов при закрытии аппы
//DynamicData тут нужна, как раз для динамического создания списка элементов
//с которыми можно взаимодействовать и их логов 
public class SimulationService : ServiceBase
{
    private const string CSEG_EXE_PATH = @"Assets\CSEG\CSEG.exe";
    private const string STARTUP_FILE_NAME = "start.xml";

    private CompositeDisposable _disposables = new();
    private readonly SourceCache<SimulationProcess, int> _processesCache = new(p => p.Id);

    public SimulationService()
    {
        _disposables.Add(
            _processesCache.Connect()
                .Bind(out var processes)
                .Subscribe());

        Processes = processes;
    }

    [Reactive]
    public ReadOnlyObservableCollection<SimulationProcess> Processes { get; set; }

    public async Task StartSimulation(string outputFolder)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = CSEG_EXE_PATH,
            Arguments = $"\"{outputFolder}\\{STARTUP_FILE_NAME}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };

        var process = new Process();

        process.StartInfo = processStartInfo;

        process.OutputDataReceived += OnOutputDataReceived;
        process.ErrorDataReceived += OnErrorDataReceived;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var simulationProcess = new SimulationProcess(process);

        _processesCache.AddOrUpdate(simulationProcess);

        await process.WaitForExitAsync();

        process.OutputDataReceived -= OnOutputDataReceived;
        process.ErrorDataReceived -= OnErrorDataReceived;

        _processesCache.Remove(simulationProcess.Id);
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (sender is Process process
            && e.Data != null
            && _processesCache.Lookup(process.Id).Value is { } simlationProcess)
        {
            simlationProcess.AppendLogString(e.Data);
        }
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (sender is Process process
            && e.Data != null
            && _processesCache.Lookup(process.Id).Value is { } simlationProcess)
        {
            simlationProcess.AppendLogString(e.Data);
        }
    }
}

public class SimulationProcess : ReactiveObject
{
    private readonly Process? _process;

    public SimulationProcess(Process? process)
    {
        _process = process;

        KillProcessCommand = ReactiveCommand.Create(KillProcess, outputScheduler: RxApp.MainThreadScheduler);

        Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).Subscribe(_ => ElapsedTime = DateTime.Now - _process.StartTime);
    }

    public ICommand KillProcessCommand { get; set; }

    private void KillProcess() => _process.Kill();

    public int Id => _process.Id;

    [Reactive]
    public TimeSpan ElapsedTime { get; set; }
    public ObservableCollection<string> Logs { get; set; } = [];

    public void AppendLogString(string info)
    {
        Logs.Insert(Logs.Count, info.Trim());
        if (Logs.Count > 50)
        {
            Logs.RemoveAt(0);
        }
    }
}