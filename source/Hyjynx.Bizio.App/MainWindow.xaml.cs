using System.Windows;
using Hyjynx.Bizio.App.ViewModels;
using Hyjynx.Wpf;

namespace Hyjynx.Bizio.App;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        var uiManager = UIManager.CaptureCurrent();
        if (!uiManager.HasUIContext)
        {
            throw new InvalidOperationException("The main window requires a UI context.");
        }

        _viewModel = new MainWindowViewModel(uiManager);
        _viewModel.QuitRequested += (_, _) => Close();
        DataContext = _viewModel;
    }
}