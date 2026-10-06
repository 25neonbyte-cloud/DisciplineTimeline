using Microsoft.Data.Sqlite;
using DisciplineTimeline.Models;

namespace DisciplineTimeline.Services;

public sealed class DatabaseService
{
    private const int CurrentSchemaVersion = 1;
    private readonly string _connectionString;

    public DatabaseService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dataDirectory = Path.Combine(appData, "DisciplineTimeline");
        Directory.CreateDirectory(dataDirectory);

        var databasePath = Path.Combine(dataDirectory, "discipline-timeline.db");
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task InitializeAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await ExecuteAsync(connection, """
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS SchemaInfo (
                Id INTEGER PRIMARY KEY CHECK (Id = 1),
                Version INTEGER NOT NULL
            );

            INSERT OR IGNORE INTO SchemaInfo (Id, Version)
            VALUES (1, 0);
            """);

        var version = await GetSchemaVersionAsync(connection);

        if (version < 1)
        {
            await ApplyVersion1Async(connection);
            await SetSchemaVersionAsync(connection, 1);
            version = 1;
        }

        if (version != CurrentSchemaVersion)
        {
            throw new InvalidOperationException(
                $"Versão de banco inesperada. Atual: {version}; esperada: {CurrentSchemaVersion}.");
        }
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksForDateAsync(DateOnly date)
    {
        var items = new List<TaskItem>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                t.Id,
                t.Title,
                t.Description,
                t.CategoryId,
                c.Name,
                t.Priority,
                t.CreatedAt,
                t.OriginalPlannedDate,
                t.CurrentPlannedDate,
                t.PlannedStartTime,
                t.PlannedEndTime,
                t.ActualStartAt,
                t.CompletedAt,
                t.Status,
                t.HasLateFlag,
                t.HasRescheduledFlag,
                t.RescheduleCount,
                t.HasCancellationFlag,
                t.RecoveredFromTaskId,
                t.CycleId,
                t.RecurrenceRule
            FROM Tasks t
            INNER JOIN Categories c ON c.Id = t.CategoryId
            WHERE t.CurrentPlannedDate = $date
            ORDER BY
                CASE WHEN t.PlannedStartTime IS NULL THEN 1 ELSE 0 END,
                t.PlannedStartTime,
                t.Id;
            """;

        command.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd"));

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new TaskItem
            {
                Id = reader.GetInt64(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                CategoryId = reader.GetInt64(3),
                CategoryName = reader.GetString(4),
                Priority = (TaskPriority)reader.GetInt32(5),
                CreatedAt = DateTime.Parse(reader.GetString(6)),
                OriginalPlannedDate = DateOnly.Parse(reader.GetString(7)),
                CurrentPlannedDate = DateOnly.Parse(reader.GetString(8)),
                PlannedStartTime = reader.IsDBNull(9) ? null : TimeOnly.Parse(reader.GetString(9)),
                PlannedEndTime = reader.IsDBNull(10) ? null : TimeOnly.Parse(reader.GetString(10)),
                ActualStartAt = reader.IsDBNull(11) ? null : DateTime.Parse(reader.GetString(11)),
                CompletedAt = reader.IsDBNull(12) ? null : DateTime.Parse(reader.GetString(12)),
                Status = (TaskStatus)reader.GetInt32(13),
                HasLateFlag = reader.GetInt32(14) == 1,
                HasRescheduledFlag = reader.GetInt32(15) == 1,
                RescheduleCount = reader.GetInt32(16),
                HasCancellationFlag = reader.GetInt32(17) == 1,
                RecoveredFromTaskId = reader.IsDBNull(18) ? null : reader.GetInt64(18),
                CycleId = reader.IsDBNull(19) ? null : reader.GetInt64(19),
                RecurrenceRule = reader.IsDBNull(20) ? null : reader.GetString(20)
            });
        }

        return items;
    }

    private static async Task ApplyVersion1Async(SqliteConnection connection)
    {
        await using var transaction = connection.BeginTransaction();

        try
        {
            await ExecuteAsync(connection, """
                CREATE TABLE IF NOT EXISTS Categories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    RecoveryPolicy TEXT NOT NULL,
                    IsSystemCategory INTEGER NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS Cycles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    StartDate TEXT NOT NULL,
                    EndDate TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Tasks (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Description TEXT NULL,
                    CategoryId INTEGER NOT NULL,
                    Priority INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL,
                    OriginalPlannedDate TEXT NOT NULL,
                    CurrentPlannedDate TEXT NOT NULL,
                    PlannedStartTime TEXT NULL,
                    PlannedEndTime TEXT NULL,
                    ActualStartAt TEXT NULL,
                    CompletedAt TEXT NULL,
                    Status INTEGER NOT NULL DEFAULT 0,
                    HasLateFlag INTEGER NOT NULL DEFAULT 0,
                    HasRescheduledFlag INTEGER NOT NULL DEFAULT 0,
                    RescheduleCount INTEGER NOT NULL DEFAULT 0,
                    HasCancellationFlag INTEGER NOT NULL DEFAULT 0,
                    RecoveredFromTaskId INTEGER NULL,
                    CycleId INTEGER NULL,
                    RecurrenceRule TEXT NULL,
                    FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
                    FOREIGN KEY (RecoveredFromTaskId) REFERENCES Tasks(Id),
                    FOREIGN KEY (CycleId) REFERENCES Cycles(Id)
                );

                CREATE INDEX IF NOT EXISTS IX_Tasks_CurrentPlannedDate
                    ON Tasks(CurrentPlannedDate);

                CREATE INDEX IF NOT EXISTS IX_Tasks_OriginalPlannedDate
                    ON Tasks(OriginalPlannedDate);

                CREATE INDEX IF NOT EXISTS IX_Tasks_Status
                    ON Tasks(Status);

                INSERT OR IGNORE INTO Categories (Name, RecoveryPolicy, IsSystemCategory)
                VALUES ('Miscelânia', 'Recoverable', 1);

                INSERT OR IGNORE INTO Categories (Name, RecoveryPolicy, IsSystemCategory)
                VALUES ('Exercícios', 'NonRecoverable', 1);
                """, transaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task<int> GetSchemaVersionAsync(SqliteConnection connection)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Version FROM SchemaInfo WHERE Id = 1;";
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task SetSchemaVersionAsync(SqliteConnection connection, int version)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE SchemaInfo SET Version = $version WHERE Id = 1;";
        command.Parameters.AddWithValue("$version", version);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task ExecuteAsync(
        SqliteConnection connection,
        string sql,
        SqliteTransaction? transaction = null)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = transaction;
        await command.ExecuteNonQueryAsync();
    }
}
