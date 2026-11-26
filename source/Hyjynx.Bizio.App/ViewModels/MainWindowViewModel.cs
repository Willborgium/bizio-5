using Hyjynx.Bizio.App.Models;
using Hyjynx.Bizio.App.Services;
using Hyjynx.Bizio.Engine;
using Hyjynx.Wpf;

namespace Hyjynx.Bizio.App.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase, IViewStateRoot<AppView>
{
    private readonly UIManager _uiManager;
    private readonly GameStorageService _storageService;
    private readonly MainMenuViewModel _mainMenuViewModel;
    private readonly GameViewModel _gameViewModel;
    private readonly LoadGameViewModel _loadGameViewModel;
    private readonly OptionsViewModel _optionsViewModel;
    private object? _currentViewModel;
    private AppView _currentView;

    public MainWindowViewModel(UIManager uiManager)
    {
        _uiManager = uiManager ?? throw new ArgumentNullException(nameof(uiManager));
        _storageService = new GameStorageService();
        MessageLayer = new MessageLayerViewModel();

        _mainMenuViewModel = new MainMenuViewModel(
            NewGame,
            ShowLoadGame,
            ShowOptions,
            RequestQuit);
        _gameViewModel = new GameViewModel(ShowMainMenu);
        _loadGameViewModel = new LoadGameViewModel(_storageService, LoadSelectedGame, ShowMainMenu);
        _optionsViewModel = new OptionsViewModel(ShowMainMenu);

        CurrentView = AppView.MainMenu;
        CurrentViewModel = _mainMenuViewModel;
    }

    public event EventHandler? QuitRequested;

    public MessageLayerViewModel MessageLayer { get; }

    public AppView CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    private void NewGame()
    {
        try
        {
            var state = EngineService.CreateNewGame();
            _storageService.SaveGame(state, _storageService.DefaultSavePath);
            _gameViewModel.ShowState(state);
            CurrentView = AppView.Game;
            CurrentViewModel = _gameViewModel;
            MessageLayer.ShowInfo("New game created", $"Saved to {_storageService.DefaultSavePath}");
        }
        catch (Exception exception)
        {
            MessageLayer.ShowError("Could not create game", exception.Message);
        }
    }

    private void ShowLoadGame()
    {
        try
        {
            _loadGameViewModel.Refresh();
            CurrentView = AppView.LoadGame;
            CurrentViewModel = _loadGameViewModel;
        }
        catch (Exception exception)
        {
            MessageLayer.ShowError("Could not list saves", exception.Message);
        }
    }

    private void LoadSelectedGame(SaveGameSummary save)
    {
        ArgumentNullException.ThrowIfNull(save);

        try
        {
            var state = _storageService.LoadGame(save.FullPath);
            _gameViewModel.ShowState(state);
            CurrentView = AppView.Game;
            CurrentViewModel = _gameViewModel;
            MessageLayer.ShowInfo("Game loaded", save.Name);
        }
        catch (Exception exception)
        {
            MessageLayer.ShowError("Could not load game", exception.Message);
        }
    }

    private void ShowOptions()
    {
        CurrentView = AppView.Options;
        CurrentViewModel = _optionsViewModel;
    }

    private void ShowMainMenu()
    {
        CurrentView = AppView.MainMenu;
        CurrentViewModel = _mainMenuViewModel;
    }

    private void RequestQuit()
    {
        if (!_uiManager.HasUIContext)
        {
            MessageLayer.ShowError("Quit failed", "No UI context is available.");
            return;
        }

        QuitRequested?.Invoke(this, EventArgs.Empty);
    }
}
