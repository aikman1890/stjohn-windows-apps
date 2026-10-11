using System.Text.Json;
using Windows.Storage;

namespace Wuse.Services;

/// <summary>
/// One memory from Shaun's family memory book.
/// </summary>
public sealed class MemoryEntry
{
    public string Date { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Tone { get; set; } = string.Empty;
    public string Sensitivity { get; set; } = "none";
    public string Photo { get; set; } = string.Empty;

    /// <summary>
    /// Entries flagged high-sensitivity are shared high-level only — never
    /// with medical or financial details.
    /// </summary>
    public bool IsHighSensitivity =>
        !string.IsNullOrWhiteSpace(Sensitivity) &&
        !Sensitivity.StartsWith("none", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// One person in the memory book.
/// </summary>
public sealed class MemoryPerson
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public List<MemoryEntry> Memories { get; } = new();
}

/// <summary>
/// A memory matched to a query, with the person it belongs to.
/// </summary>
public sealed class MemoryMatch
{
    public MemoryPerson Person { get; set; } = new();
    public MemoryEntry Entry { get; set; } = new();
    public int Score { get; set; }
}

/// <summary>
/// Loads Shaun's family memory book (FamilyMemories.json) from the app's
/// LocalFolder and answers read-only queries against it. This is the legacy
/// feature: family members can ask wuse about Shaun's memories with them.
///
/// Privacy: the file NEVER leaves this device. Memory content is never
/// written to diagnostic logs or files by this service, never sent to any
/// network API, and never used for training. If the file is absent, every
/// method quietly returns empty and the feature disables itself.
/// </summary>
public sealed class MemoryBookService
{
    public static MemoryBookService Instance { get; } = new MemoryBookService();

    private const string FileName = "FamilyMemories.json";

    private readonly List<MemoryPerson> _people = new();
    private readonly Random _random = new();
    private bool _loaded;
    private bool _loadAttempted;

    /// <summary>True when a memory book was found and loaded.</summary>
    public bool IsAvailable => _loaded && _people.Count > 0;

    /// <summary>Fires once the initial load attempt finishes.</summary>
    public event EventHandler? Loaded;

    public IReadOnlyList<MemoryPerson> People => _people;

    private MemoryBookService()
    {
    }

    /// <summary>
    /// Loads the memory book from LocalFolder. Call once at app startup
    /// (fire-and-forget is fine); queries before load completes return empty.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_loadAttempted)
            return;
        _loadAttempted = true;
        try
        {
            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.TryGetItemAsync(FileName) as StorageFile;
            if (file == null)
                return; // No memory book installed — feature stays off, silently.
            var json = await FileIO.ReadTextAsync(file);
            Parse(json);
        }
        catch
        {
            // Corrupt or unreadable file: stay disabled, never crash the app.
            _people.Clear();
            _loaded = false;
        }
        finally
        {
            Loaded?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Parse(string json)
    {
        _people.Clear();
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("people", out var peopleEl))
            return;
        foreach (var prop in peopleEl.EnumerateObject())
        {
            var person = new MemoryPerson { Key = prop.Name };
            var el = prop.Value;
            person.Name = GetString(el, "name");
            person.Nickname = GetString(el, "nickname");
            person.Relationship = GetString(el, "relationship");
            if (el.TryGetProperty("memories", out var mems))
            {
                foreach (var m in mems.EnumerateArray())
                {
                    person.Memories.Add(new MemoryEntry
                    {
                        Date = GetString(m, "date"),
                        Title = GetString(m, "title"),
                        Text = GetString(m, "text"),
                        Tone = GetString(m, "tone"),
                        Sensitivity = GetString(m, "sensitivity"),
                        Photo = GetString(m, "photo"),
                    });
                }
            }
            if (person.Memories.Count > 0)
                _people.Add(person);
        }
        _loaded = _people.Count > 0;
    }

    private static string GetString(JsonElement el, string name) =>
        el.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? string.Empty
            : string.Empty;

    /// <summary>
    /// Keyword search across titles, texts, tones, person names, nicknames,
    /// and relationships. Ranked by keyword hits, best first.
    /// </summary>
    public IReadOnlyList<MemoryMatch> SearchMemories(string query)
    {
        var results = new List<MemoryMatch>();
        try
        {
            if (!_loaded)
                return results;
            var keywords = Tokenize(query);
            if (keywords.Count == 0)
                return results;
            foreach (var person in _people)
            {
                var personText = $"{person.Name} {person.Nickname} {person.Relationship} {person.Key}".ToLowerInvariant();
                foreach (var entry in person.Memories)
                {
                    var haystack = $"{entry.Title} {entry.Text} {entry.Tone} {personText}".ToLowerInvariant();
                    int score = keywords.Count(k => haystack.Contains(k, StringComparison.Ordinal));
                    // A query naming the person matches all of their memories.
                    if (score > 0)
                        results.Add(new MemoryMatch { Person = person, Entry = entry, Score = score });
                }
            }
            results.Sort((a, b) => b.Score.CompareTo(a.Score));
        }
        catch
        {
            // Search must never throw.
        }
        return results;
    }

    /// <summary>
    /// All memories for a person, matched by key, name, nickname, or common
    /// alias ("mom", "dj", ...). Returns empty when no one matches.
    /// </summary>
    public IReadOnlyList<MemoryMatch> MemoriesFor(string personQuery)
    {
        var results = new List<MemoryMatch>();
        try
        {
            if (!_loaded)
                return results;
            var person = FindPerson(personQuery);
            if (person == null)
                return results;
            results.AddRange(person.Memories.Select(e =>
                new MemoryMatch { Person = person, Entry = e, Score = 1 }));
        }
        catch
        {
        }
        return results;
    }

    /// <summary>
    /// Memories from a given month and day (any year) — "on this day".
    /// </summary>
    public IReadOnlyList<MemoryMatch> MemoriesOn(DateTime date)
    {
        var results = new List<MemoryMatch>();
        try
        {
            if (!_loaded)
                return results;
            foreach (var person in _people)
            {
                foreach (var entry in person.Memories)
                {
                    if (TryParseDate(entry.Date, out var d) &&
                        d.Month == date.Month && d.Day == date.Day)
                        results.Add(new MemoryMatch { Person = person, Entry = entry, Score = 1 });
                }
            }
        }
        catch
        {
        }
        return results;
    }

    /// <summary>A random memory, for "tell me a memory".</summary>
    public MemoryMatch? RandomMemory()
    {
        try
        {
            if (!_loaded)
                return null;
            var all = _people.SelectMany(p => p.Memories.Select(e =>
                new MemoryMatch { Person = p, Entry = e, Score = 1 })).ToList();
            if (all.Count == 0)
                return null;
            return all[_random.Next(all.Count)];
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Matches a free-text person reference to a person in the book.
    /// </summary>
    public MemoryPerson? FindPerson(string query)
    {
        try
        {
            if (!_loaded || string.IsNullOrWhiteSpace(query))
                return null;
            var q = query.Trim().ToLowerInvariant();
            foreach (var person in _people)
            {
                var key = person.Key.ToLowerInvariant();
                if (q.Contains(key, StringComparison.Ordinal))
                    return person;
                if (!string.IsNullOrWhiteSpace(person.Name) &&
                    q.Contains(person.Name.ToLowerInvariant(), StringComparison.Ordinal))
                    return person;
                if (!string.IsNullOrWhiteSpace(person.Nickname) &&
                    q.Contains(person.Nickname.ToLowerInvariant(), StringComparison.Ordinal))
                    return person;
                foreach (var alias in AliasesFor(key))
                {
                    if (q.Contains(alias, StringComparison.Ordinal))
                        return person;
                }
            }
        }
        catch
        {
        }
        return null;
    }

    private static IEnumerable<string> AliasesFor(string key) => key switch
    {
        "donna" => new[] { "mom", "mother", "mama", "momma" },
        "dj" => new[] { "dwayne" },
        "matt" => new[] { "matthew" },
        "kathryn" => new[] { "katie", "kat" },
        _ => Array.Empty<string>(),
    };

    private static List<string> Tokenize(string query) =>
        query.ToLowerInvariant()
            .Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '\'', '"' },
                StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .Distinct()
            .ToList();

    private static bool TryParseDate(string text, out DateTime date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;
        return DateTime.TryParse(text, out date);
    }
}
