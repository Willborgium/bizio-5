using System.Collections.ObjectModel;
using System.Windows.Input;
using Hyjynx.Bizio.App.Models;
using Hyjynx.Bizio.App.Services;
using Hyjynx.Wpf;

namespace Hyjynx.Bizio.App.ViewModels;

public sealed class LoadGameViewModel : ViewModelBase
{
    private readonly GameStorageService _storageService;
    private readonly Action<SaveGameSummary> _loadRequested;
    private readonly Action _backRequested;
    private SaveGameSummary? _selectedSave;

    public LoadGameViewModel(
        GameStorageService storageService,
        Action<SaveGameSummary> loadRequested,
        Action backRequested)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _loadRequested = loadRequested ?? throw new ArgumentNullException(nameof(loadRequested));
        _backRequested = backRequested ?? throw new ArgumentNullException(nameof(backRequested));

        RefreshCommand = GetCommand(Refresh);
        LoadSelectedCommand = GetCommand(LoadSelected, _ => SelectedSave is not null);
        BackCommand = GetCommand(_backRequested);
    }

    public ObservableCollection<SaveGameSummary> Saves { get; } = new();
    public ICommand RefreshCommand { get; }
    public ICommand LoadSelectedCommand { get; }
    public ICommand BackCommand { get; }

    public SaveGameSummary? SelectedSave
    {
        get => _selectedSave;
        set
        {
            if (!SetProperty(ref _selectedSave, value))
            {
                return;
            }

            CommandManager.InvalidateRequerySuggested();
        }
    }

    public void Refresh()
    {
        Saves.Clear();
        foreach (var save in _storageService.ListSaveGames())
        {
            Saves.Add(save);
        }

        SelectedSave = Saves.FirstOrDefault();
    }

    private void LoadSelected()
    {
        if (SelectedSave is null)
        {
            return;
        }

        _loadRequested(SelectedSave);
    }
}
