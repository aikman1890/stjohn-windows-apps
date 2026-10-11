using System.Text.Json;
using Windows.Storage;
using Wuse.Models;

namespace Wuse.Services;

/// <summary>
/// Persists conversations as JSON in the app's local folder (conversations.json).
/// </summary>
public sealed class ConversationStore
{
    private const string FileName = "conversations.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly List<Conversation> _conversations = new();
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _loaded;

    public IReadOnlyList<Conversation> Conversations => _conversations;

    public async Task LoadAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_loaded) return;
            _loaded = true;

            try
            {
                var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(FileName);
                if (item is StorageFile file)
                {
                    var json = await FileIO.ReadTextAsync(file);
                    var stored = JsonSerializer.Deserialize<List<Conversation>>(json, JsonOptions);
                    if (stored is not null)
                    {
                        _conversations.Clear();
                        _conversations.AddRange(stored.OrderByDescending(c => c.StartedAt));
                    }
                }
            }
            catch
            {
                // A missing or corrupt file just means no saved conversations yet.
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Creates, stores, and returns a new conversation titled with the
    /// first 40 characters of the first user message.
    /// </summary>
    public Conversation NewConversation(string firstUserMessage)
    {
        var text = (firstUserMessage ?? string.Empty).Trim();
        if (text.Length == 0) text = "New chat";
        var title = text.Length <= 40 ? text : text.Substring(0, 40).TrimEnd() + "…";

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Title = title,
            StartedAt = DateTime.Now,
        };
        _conversations.Insert(0, conversation);
        _ = SaveAsync();
        return conversation;
    }

    public void AddMessage(Conversation conversation, ChatMessage message)
    {
        conversation.Messages.Add(message);
        _ = SaveAsync();
    }

    public async Task DeleteConversationAsync(Guid id)
    {
        var existing = _conversations.FirstOrDefault(c => c.Id == id);
        if (existing is not null)
        {
            _conversations.Remove(existing);
            await SaveAsync();
        }
    }

    public async Task RenameConversationAsync(Guid id, string newTitle)
    {
        var existing = _conversations.FirstOrDefault(c => c.Id == id);
        if (existing is not null)
        {
            existing.Title = newTitle;
            await SaveAsync();
        }
    }

    private async Task SaveAsync()
    {
        await _gate.WaitAsync();
        try
        {
            try
            {
                var json = JsonSerializer.Serialize(_conversations, JsonOptions);
                var file = await ApplicationData.Current.LocalFolder
                    .CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, json);
            }
            catch
            {
                // Saving is best-effort; the in-memory conversation still works.
            }
        }
        finally
        {
            _gate.Release();
        }
    }
}
