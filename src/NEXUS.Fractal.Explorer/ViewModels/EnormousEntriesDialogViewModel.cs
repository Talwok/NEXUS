using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using NEXUS.BaseClasses;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace NEXUS.Fractal.Explorer.ViewModels;

public partial class EnormousEntriesDialogViewModel : ObservableBaseObject, IDialogAware
{
    [ObservableProperty]
    private int _entriesCount;

    [ObservableProperty]
    private string _title;
    
    [ObservableProperty]
    private string _message;
    
    public EnormousEntriesDialogViewModel()
    {
        CloseCommand = new DelegateCommand(OnCloseDialog);
    }

    private void OnCloseDialog()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
    }

    public ICommand CloseCommand { get; }
    
    public bool CanCloseDialog() => true;

    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        EntriesCount = parameters.GetValue<int>("entryCount");
        Title = "Внимание!";
        Message = $"Найдено избыточно много записей, всего: {EntriesCount}";
    }
    
    public event Action<IDialogResult> RequestClose;
}