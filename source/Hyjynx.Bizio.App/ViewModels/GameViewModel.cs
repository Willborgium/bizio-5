using System.Windows.Input;
using Hyjynx.Bizio.Engine;
using Hyjynx.Wpf;

namespace Hyjynx.Bizio.App.ViewModels;

public sealed class GameViewModel : ViewModelBase
{
    private GameState? _state;
    private string _turnText = "Turn -";

    public GameViewModel(Action mainMenuRequested)
    {
        ArgumentNullException.ThrowIfNull(mainMenuRequested);
        MainMenuCommand = GetCommand(mainMenuRequested);
    }

    public ICommand MainMenuCommand { get; }
    public string TurnText
    {
        get => _turnText;
        private set => SetProperty(ref _turnText, value);
    }

    public void ShowState(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        _state = state;
        TurnText = $"Turn {_state.Turn}";
    }
}
