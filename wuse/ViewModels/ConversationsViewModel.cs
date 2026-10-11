using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wuse.Models;
using Wuse.Services;

namespace Wuse.ViewModels;

public partial class ConversationsViewModel : ObservableObject
{
    private readonly ConversationStore _store;
    private bool _initialized;

    public ObservableCollection<Conversation> Conversations { get; } = new();

    /// <summary>
    /// Wired by the page to navigate back to chat. Null starts a new chat.
    /// </summary>
    public Action<Conversation?>? NavigateToChat { get; set; }

    public ConversationsViewModel(ConversationStore store)
    {
        _store = store;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;
        await _store.LoadAsync();
        RefreshList();
    }

    public void RefreshList()
    {
        Conversations.Clear();
        foreach (var conversation in _store.Conversations.OrderByDescending(c => c.StartedAt))
            Conversations.Add(conversation);
    }

    [RelayCommand]
    private void New() => NavigateToChat?.Invoke(null);

    public void Open(Conversation conversation) => NavigateToChat?.Invoke(conversation);

    public async Task DeleteAsync(Conversation conversation)
    {
        await _store.DeleteConversationAsync(conversation.Id);
        Conversations.Remove(conversation);
    }

    public async Task RenameAsync(Conversation conversation, Func<string, Task<string?>> promptForTitle)
    {
        var result = await promptForTitle(conversation.Title);
        var title = (result ?? string.Empty).Trim();
        if (title.Length == 0 || title == conversation.Title) return;

        await _store.RenameConversationAsync(conversation.Id, title);
        conversation.Title = title;
        RefreshList();
    }
}
