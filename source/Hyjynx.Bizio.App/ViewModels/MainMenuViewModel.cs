using System.Windows.Input;
using Hyjynx.Wpf;

namespace Hyjynx.Bizio.App.ViewModels;

public sealed class MainMenuViewModel : ViewModelBase
{
    public MainMenuViewModel(
        Action newGameRequested,
        Action loadGameRequested,
        Action optionsRequested,
        Action quitRequested)
    {
        ArgumentNullException.ThrowIfNull(newGameRequested);
        ArgumentNullException.ThrowIfNull(loadGameRequested);
        ArgumentNullException.ThrowIfNull(optionsRequested);
        ArgumentNullException.ThrowIfNull(quitRequested);

        NewGameCommand = GetCommand(newGameRequested);
        LoadGameCommand = GetCommand(loadGameRequested);
        OptionsCommand = GetCommand(optionsRequested);
        QuitCommand = GetCommand(quitRequested);
    }

    public ICommand NewGameCommand { get; }
    public ICommand LoadGameCommand { get; }
    public ICommand OptionsCommand { get; }
    public ICommand QuitCommand { get; }
}
