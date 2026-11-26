using System.Windows.Input;
using Hyjynx.Wpf;

namespace Hyjynx.Bizio.App.ViewModels;

public sealed class OptionsViewModel : ViewModelBase
{
    public OptionsViewModel(Action backRequested)
    {
        ArgumentNullException.ThrowIfNull(backRequested);
        BackCommand = GetCommand(backRequested);
    }

    public ICommand BackCommand { get; }
}
