using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DisciplineTimeline.Tests;

public sealed class MetricsRepositoryIntegrationTests
{
    [Fact]
    public async Task ReadRepository_IncludesRecoveryOutsidePeriodForOriginalPlan()
    {
        var cancellation = TestContext.Current.CancellationToken;
        var path = Path.Combine(
            Path.GetTempPath(), $"discipline-metrics-{Guid.NewGuid():N}.db");

        try
        {
            var options = new DbContextOptionsBuilder<DisciplineTimelineDbContext>()
                .UseSqlite($"Data Source={path};Pooling=False")
                .Options;
            var factory = new TestContextFactory(options);
            long originalId;

            await using (var db = new DisciplineTimelineDbContext(options))
            {
                await db.Database.EnsureCreatedAsync(cancellation);
                var lost = NewTask(1, new DateOnly(2026, 10, 8), TaskState.Lost);
                db.Tasks.Add(lost);
                await db.SaveChangesAsync(cancellation);
                originalId = lost.Id;

                var bonus = NewTask(1, new DateOnly(2026, 11, 4), TaskState.Completed);
                bonus.RecoveredFromTaskId = originalId;
                bonus.CompletedAt = new DateTimeOffset(2026, 11, 4, 18, 0, 0, TimeSpan.FromHours(-3));
                bonus.Origin = "RecoveredBonus";
                db.Tasks.Add(bonus);
                await db.SaveChangesAsync(cancellation);
            }

            var repository = new EfMetricsReadRepository(factory);
            var tasks = await repository.GetReportTasksAsync(
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 10, 31),
                cancellation);

            Assert.Equal(2, tasks.Count);
            Assert.Contains(tasks, t => t.RecoveredFromTaskId == originalId);
            Assert.Single(tasks.Where(t => t.Status == TaskState.Lost));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static TaskItem NewTask(long categoryId, DateOnly date, TaskState status)
        => new()
        {
            CategoryId = categoryId,
            Title = "Teste",
            CreatedAt = new DateTimeOffset(2026, 10, 8, 8, 0, 0, TimeSpan.FromHours(-3)),
            OriginalPlannedDate = date,
            CurrentPlannedDate = date,
            Status = status,
            Origin = "Manual"
        };

    private sealed class TestContextFactory : IDbContextFactory<DisciplineTimelineDbContext>
    {
        private readonly DbContextOptions<DisciplineTimelineDbContext> _options;

        public TestContextFactory(DbContextOptions<DisciplineTimelineDbContext> options)
            => _options = options;

        public DisciplineTimelineDbContext CreateDbContext()
            => new(_options);

        public Task<DisciplineTimelineDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDbContext());
    }
}
