using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DisciplineTimeline.Tests;

public sealed class TaskRepositoryIntegrationTests
{
    [Fact]
    public async Task Repository_PerformsCrudAndTemporalQueriesAgainstSqlite()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"discipline-timeline-crud-{Guid.NewGuid():N}.db");

        try
        {
            var options = new DbContextOptionsBuilder<DisciplineTimelineDbContext>()
                .UseSqlite($"Data Source={databasePath};Pooling=False")
                .Options;

            var factory = new TestDbContextFactory(options);

            await using (var db = await factory.CreateDbContextAsync(cancellationToken))
            {
                await db.Database.EnsureCreatedAsync(cancellationToken);
            }

            var repository = new EfTaskRepository(factory);

            var task = new TaskItem
            {
                Title = "CRUD",
                CategoryId = 1,
                Priority = TaskPriority.Normal,
                CreatedAt = new DateTimeOffset(2026, 10, 6, 10, 0, 0, TimeSpan.Zero),
                OriginalPlannedDate = new DateOnly(2026, 10, 6),
                CurrentPlannedDate = new DateOnly(2026, 10, 6),
                Status = TaskState.Planned,
                Origin = "Manual"
            };

            var created = await repository.AddAsync(task, cancellationToken);
            Assert.True(created.Id > 0);
            Assert.Equal("Miscelânia", created.Category?.Name);

            var open = await repository.GetOpenAsync(cancellationToken);
            Assert.Contains(open, x => x.Id == created.Id);

            var loaded = await repository.GetByIdAsync(created.Id, cancellationToken);
            Assert.NotNull(loaded);

            loaded.Title = "CRUD atualizado";
            await repository.UpdateAsync(loaded, cancellationToken);

            var updated = await repository.GetByIdAsync(created.Id, cancellationToken);
            Assert.Equal("CRUD atualizado", updated?.Title);

            await repository.DeleteAsync(updated!, cancellationToken);

            var deleted = await repository.GetByIdAsync(created.Id, cancellationToken);
            Assert.Null(deleted);
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    [Fact]
    public async Task Repository_TracksRecurrenceAndRecoveryWithoutSchemaExpansion()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"discipline-timeline-history-{Guid.NewGuid():N}.db");

        try
        {
            var options = new DbContextOptionsBuilder<DisciplineTimelineDbContext>()
                .UseSqlite($"Data Source={databasePath};Pooling=False")
                .Options;

            var factory = new TestDbContextFactory(options);

            await using (var db = await factory.CreateDbContextAsync(cancellationToken))
            {
                await db.Database.EnsureCreatedAsync(cancellationToken);
            }

            var repository = new EfTaskRepository(factory);

            var root = await repository.AddAsync(
                new TaskItem
                {
                    Title = "Recorrente",
                    CategoryId = 1,
                    CreatedAt = DateTimeOffset.UtcNow,
                    OriginalPlannedDate = new DateOnly(2026, 10, 6),
                    CurrentPlannedDate = new DateOnly(2026, 10, 6),
                    Status = TaskState.Planned,
                    Origin = "RecurrenceRoot",
                    RecurrenceRule = "FREQ=DAILY"
                },
                cancellationToken);

            await repository.AddAsync(
                new TaskItem
                {
                    Title = "Ocorrência",
                    CategoryId = 1,
                    CreatedAt = DateTimeOffset.UtcNow,
                    OriginalPlannedDate = new DateOnly(2026, 10, 7),
                    CurrentPlannedDate = new DateOnly(2026, 10, 7),
                    Status = TaskState.Planned,
                    Origin = $"Recurrence:{root.Id}",
                    RecurrenceRule = "FREQ=DAILY"
                },
                cancellationToken);

            Assert.True(await repository.RecurrenceOccurrenceExistsAsync(
                root.Id,
                new DateOnly(2026, 10, 7),
                cancellationToken));

            var lost = await repository.AddAsync(
                new TaskItem
                {
                    Title = "Perdida",
                    CategoryId = 1,
                    CreatedAt = DateTimeOffset.UtcNow,
                    OriginalPlannedDate = new DateOnly(2026, 10, 5),
                    CurrentPlannedDate = new DateOnly(2026, 10, 5),
                    Status = TaskState.Lost,
                    Origin = "Manual"
                },
                cancellationToken);

            await repository.AddAsync(
                new TaskItem
                {
                    Title = "Bônus",
                    CategoryId = 1,
                    CreatedAt = DateTimeOffset.UtcNow,
                    OriginalPlannedDate = new DateOnly(2026, 10, 6),
                    CurrentPlannedDate = new DateOnly(2026, 10, 6),
                    Status = TaskState.Completed,
                    Origin = "RecoveredBonus",
                    RecoveredFromTaskId = lost.Id,
                    CompletedAt = DateTimeOffset.UtcNow
                },
                cancellationToken);

            Assert.True(await repository.RecoveryExistsAsync(
                lost.Id,
                cancellationToken));
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }

    private sealed class TestDbContextFactory : IDbContextFactory<DisciplineTimelineDbContext>
    {
        private readonly DbContextOptions<DisciplineTimelineDbContext> _options;

        public TestDbContextFactory(DbContextOptions<DisciplineTimelineDbContext> options)
        {
            _options = options;
        }

        public DisciplineTimelineDbContext CreateDbContext()
            => new(_options);

        public Task<DisciplineTimelineDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(new DisciplineTimelineDbContext(_options));
    }
}
