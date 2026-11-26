using System.Windows.Input;
using Hyjynx.Wpf;

namespace Hyjynx.Bizio.App.ViewModels;

public sealed class MessageLayerViewModel : ViewModelBase
{
    private readonly Queue<MessageEntry> _queue = new();
    private MessageEntry? _currentMessage;

    public MessageLayerViewModel()
    {
        DismissCommand = GetCommand(DismissCurrent);
    }

    public ICommand DismissCommand { get; }

    public bool HasMessage => _currentMessage is not null;
    public string MessageTitle => _currentMessage?.Title ?? string.Empty;
    public string MessageBody => _currentMessage?.Body ?? string.Empty;
    public bool IsError => _currentMessage?.IsError is true;

    public void ShowInfo(string title, string body)
    {
        Enqueue(new MessageEntry(title, body, IsError: false));
    }

    public void ShowError(string title, string body)
    {
        Enqueue(new MessageEntry(title, body, IsError: true));
    }

    private void Enqueue(MessageEntry entry)
    {
        if (_currentMessage is null)
        {
            _currentMessage = entry;
            RaiseMessagePropertiesChanged();
            return;
        }

        _queue.Enqueue(entry);
    }

    private void DismissCurrent()
    {
        _currentMessage = _queue.Count > 0
            ? _queue.Dequeue()
            : null;
        RaiseMessagePropertiesChanged();
    }

    private void RaiseMessagePropertiesChanged()
    {
        OnPropertyChanged(nameof(HasMessage));
        OnPropertyChanged(nameof(MessageTitle));
        OnPropertyChanged(nameof(MessageBody));
        OnPropertyChanged(nameof(IsError));
    }

    private sealed record MessageEntry(string Title, string Body, bool IsError);
}
