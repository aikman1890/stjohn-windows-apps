using ClaimPilot.Models;
using Microsoft.Data.Sqlite;
using Windows.Storage;

namespace ClaimPilot.Services;

/// <summary>
/// SQLite storage for deadlines, evidence items, and timeline events.
/// Database file lives at LocalFolder\claimpilot.db. All queries are parameterized.
/// </summary>
public sealed class DatabaseService
{
    private static readonly Lazy<DatabaseService> _instance = new(() => new DatabaseService());
    public static DatabaseService Instance => _instance.Value;

    private readonly string _dbPath;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    private DatabaseService()
    {
        _dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "claimpilot.db");
    }

    public async Task InitializeAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            if (_initialized)
            {
                return;
            }

            await using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            const string sql = """
                CREATE TABLE IF NOT EXISTS Deadlines (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    DueDate TEXT NOT NULL,
                    Notes TEXT NOT NULL DEFAULT '',
                    Category TEXT NOT NULL DEFAULT 'Other',
                    IsDone INTEGER NOT NULL DEFAULT 0,
                    NotifiedKey TEXT NOT NULL DEFAULT ''
                );
                CREATE TABLE IF NOT EXISTS EvidenceItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Category TEXT NOT NULL DEFAULT '',
                    ItemDate TEXT NOT NULL,
                    Description TEXT NOT NULL DEFAULT '',
                    FilePath TEXT NOT NULL DEFAULT ''
                );
                CREATE TABLE IF NOT EXISTS TimelineEvents (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EventDate TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    Details TEXT NOT NULL DEFAULT '',
                    Category TEXT NOT NULL DEFAULT ''
                );
                CREATE TABLE IF NOT EXISTS Courses (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Chapter INTEGER NOT NULL DEFAULT 7,
                    CourseType TEXT NOT NULL DEFAULT '',
                    Status TEXT NOT NULL DEFAULT 'NotStarted',
                    ScheduledDate TEXT NOT NULL,
                    CompletedDate TEXT NOT NULL,
                    ProviderName TEXT NOT NULL DEFAULT '',
                    CertificateFileName TEXT NOT NULL DEFAULT ''
                );
                """;

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_initialized)
        {
            await InitializeAsync();
        }
    }

    // ---------- Deadlines ----------

    public async Task<List<DeadlineItem>> GetDeadlinesAsync()
    {
        await EnsureInitializedAsync();
        var items = new List<DeadlineItem>();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, DueDate, Notes, Category, IsDone, NotifiedKey FROM Deadlines ORDER BY DueDate ASC;";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var item = new DeadlineItem
            {
                Id = reader.GetInt64(0),
                Title = reader.GetString(1),
                DueDate = DateTime.Parse(reader.GetString(2), null, System.Globalization.DateTimeStyles.RoundtripKind),
                Notes = reader.GetString(3),
                Category = reader.GetString(4),
                NotifiedKey = reader.GetString(6)
            };
            // Set IsDone via property so no extra notification logic is needed here.
            item.IsDone = reader.GetInt64(5) == 1;
            items.Add(item);
        }
        return items;
    }

    public async Task<long> AddDeadlineAsync(DeadlineItem item)
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Deadlines (Title, DueDate, Notes, Category, IsDone, NotifiedKey) VALUES (@title, @due, @notes, @cat, @done, @key); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("@title", item.Title);
        command.Parameters.AddWithValue("@due", item.DueDate.ToString("o"));
        command.Parameters.AddWithValue("@notes", item.Notes);
        command.Parameters.AddWithValue("@cat", item.Category);
        command.Parameters.AddWithValue("@done", item.IsDone ? 1 : 0);
        command.Parameters.AddWithValue("@key", item.NotifiedKey);
        var id = (long)(await command.ExecuteScalarAsync())!;
        item.Id = id;
        return id;
    }

    public async Task UpdateDeadlineAsync(DeadlineItem item)
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Deadlines SET Title = @title, DueDate = @due, Notes = @notes, Category = @cat, IsDone = @done, NotifiedKey = @key WHERE Id = @id;";
        command.Parameters.AddWithValue("@title", item.Title);
        command.Parameters.AddWithValue("@due", item.DueDate.ToString("o"));
        command.Parameters.AddWithValue("@notes", item.Notes);
        command.Parameters.AddWithValue("@cat", item.Category);
        command.Parameters.AddWithValue("@done", item.IsDone ? 1 : 0);
        command.Parameters.AddWithValue("@key", item.NotifiedKey);
        command.Parameters.AddWithValue("@id", item.Id);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteDeadlineAsync(long id)
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Deadlines WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    // ---------- Evidence items ----------

    public async Task<List<EvidenceItem>> GetEvidenceItemsAsync()
    {
        await EnsureInitializedAsync();
        var items = new List<EvidenceItem>();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Category, ItemDate, Description, FilePath FROM EvidenceItems ORDER BY ItemDate DESC;";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new EvidenceItem
            {
                Id = reader.GetInt64(0),
                Title = reader.GetString(1),
                Category = reader.GetString(2),
                ItemDate = DateTime.Parse(reader.GetString(3), null, System.Globalization.DateTimeStyles.RoundtripKind),
                Description = reader.GetString(4),
                FilePath = reader.GetString(5)
            });
        }
        return items;
    }

    public async Task<long> AddEvidenceItemAsync(EvidenceItem item)
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO EvidenceItems (Title, Category, ItemDate, Description, FilePath) VALUES (@title, @cat, @date, @desc, @path); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("@title", item.Title);
        command.Parameters.AddWithValue("@cat", item.Category);
        command.Parameters.AddWithValue("@date", item.ItemDate.ToString("o"));
        command.Parameters.AddWithValue("@desc", item.Description);
        command.Parameters.AddWithValue("@path", item.FilePath);
        var id = (long)(await command.ExecuteScalarAsync())!;
        item.Id = id;
        return id;
    }

    public async Task DeleteEvidenceItemAsync(long id)
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM EvidenceItems WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    // ---------- Timeline events ----------

    public async Task<List<TimelineEvent>> GetTimelineEventsAsync()
    {
        await EnsureInitializedAsync();
        var items = new List<TimelineEvent>();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, EventDate, Title, Details, Category FROM TimelineEvents ORDER BY EventDate ASC;";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new TimelineEvent
            {
                Id = reader.GetInt64(0),
                EventDate = DateTime.Parse(reader.GetString(1), null, System.Globalization.DateTimeStyles.RoundtripKind),
                Title = reader.GetString(2),
                Details = reader.GetString(3),
                Category = reader.GetString(4)
            });
        }
        return items;
    }

    public async Task<long> AddTimelineEventAsync(TimelineEvent item)
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO TimelineEvents (EventDate, Title, Details, Category) VALUES (@date, @title, @details, @cat); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("@date", item.EventDate.ToString("o"));
        command.Parameters.AddWithValue("@title", item.Title);
        command.Parameters.AddWithValue("@details", item.Details);
        command.Parameters.AddWithValue("@cat", item.Category);
        var id = (long)(await command.ExecuteScalarAsync())!;
        item.Id = id;
        return id;
    }

    public async Task DeleteTimelineEventAsync(long id)
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM TimelineEvents WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    // ---------- Bankruptcy course tracking ----------

    public async Task<List<CourseRecord>> GetCourseRecordsAsync()
    {
        await EnsureInitializedAsync();
        var items = new List<CourseRecord>();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Chapter, CourseType, Status, ScheduledDate, CompletedDate, ProviderName, CertificateFileName FROM Courses ORDER BY Chapter ASC, CourseType ASC;";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new CourseRecord
            {
                Id = reader.GetInt64(0),
                Chapter = reader.GetInt32(1),
                CourseType = reader.GetString(2),
                Status = reader.GetString(3),
                ScheduledDate = DateTime.Parse(reader.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind),
                CompletedDate = DateTime.Parse(reader.GetString(5), null, System.Globalization.DateTimeStyles.RoundtripKind),
                ProviderName = reader.GetString(6),
                CertificateFileName = reader.GetString(7)
            });
        }
        return items;
    }

    /// <summary>Seeds the four course rows (2 courses x 2 chapters) on first run.</summary>
    public async Task EnsureCourseRecordsSeededAsync()
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        foreach (var chapter in new[] { 7, 13 })
        {
            foreach (var courseType in new[] { "CreditCounseling", "DebtorEducation" })
            {
                await using var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Courses (Chapter, CourseType, Status, ScheduledDate, CompletedDate, ProviderName, CertificateFileName) " +
                    "SELECT @chapter, @type, 'NotStarted', @today, @today, '', '' " +
                    "WHERE NOT EXISTS (SELECT 1 FROM Courses WHERE Chapter = @chapter AND CourseType = @type);";
                command.Parameters.AddWithValue("@chapter", chapter);
                command.Parameters.AddWithValue("@type", courseType);
                command.Parameters.AddWithValue("@today", DateTime.Today.ToString("o"));
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task UpdateCourseRecordAsync(CourseRecord record)
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Courses SET Status = @status, ScheduledDate = @sched, CompletedDate = @done, ProviderName = @provider, CertificateFileName = @cert WHERE Id = @id;";
        command.Parameters.AddWithValue("@status", record.Status);
        command.Parameters.AddWithValue("@sched", record.ScheduledDate.ToString("o"));
        command.Parameters.AddWithValue("@done", record.CompletedDate.ToString("o"));
        command.Parameters.AddWithValue("@provider", record.ProviderName);
        command.Parameters.AddWithValue("@cert", record.CertificateFileName);
        command.Parameters.AddWithValue("@id", record.Id);
        await command.ExecuteNonQueryAsync();
    }

    public async Task ResetCourseRecordAsync(CourseRecord record)
    {
        await EnsureInitializedAsync();
        await using var connection = new SqliteConnection($"Data Source={_dbPath}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Courses SET Status = 'NotStarted', ScheduledDate = @today, CompletedDate = @today, ProviderName = '', CertificateFileName = '' WHERE Id = @id;";
        command.Parameters.AddWithValue("@today", DateTime.Today.ToString("o"));
        command.Parameters.AddWithValue("@id", record.Id);
        await command.ExecuteNonQueryAsync();
    }
}
