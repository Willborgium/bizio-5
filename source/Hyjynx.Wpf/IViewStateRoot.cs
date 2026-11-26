namespace Hyjynx.Wpf;

public interface IViewStateRoot<TView>
{
    TView CurrentView { get; set; }
    object? CurrentViewModel { get; set; }
}
