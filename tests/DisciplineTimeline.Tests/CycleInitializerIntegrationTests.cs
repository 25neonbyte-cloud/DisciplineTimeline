using DisciplineTimeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace DisciplineTimeline.Tests;

public sealed class CycleInitializerIntegrationTests
{
    [Fact]
    public async Task Bootstrap_SeedsInitialCycleExactlyOnce()
    {
        var ct = TestContext.Current.CancellationToken;
        var path = Path.Combine(
            Path.GetTempPath(), $"discipline-cycle-{Guid.NewGuid():N}.db");

        try
        {
            var options = new DbContextOptionsBuilder<DisciplineTimelineDbContext>()
                .UseSqlite($"Data Source={path};Pooling=False")
                .Options;

            var clock = new FakeTimeProvider(
                new DateTimeOffset(2026, 10, 6, 12, 0, 0, TimeSpan.FromHours(-3)));
            var factory = new LocalFactory(options);
            var initializer = new DatabaseInitializer(factory, clock);

            await initializer.InitializeAsync(ct);
            await initializer.InitializeAsync(ct);

            await using var db = new DisciplineTimelineDbContext(options);
            var cycles = await db.Cycles.ToListAsync(ct);

            var cycle = Assert.Single(cycles);
            Assert.Equal(new DateOnly(2026, 10, 6), cycle.StartDate);
            Assert.Equal(new DateOnly(2026, 10, 25), cycle.EndDate);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private sealed class LocalFactory : IDbContextFactory<DisciplineTimelineDbContext>
    {
        private readonly DbContextOptions<DisciplineTimelineDbContext> _options;

        public LocalFactory(DbContextOptions<DisciplineTimelineDbContext> options)
            => _options = options;

        public DisciplineTimelineDbContext CreateDbContext()
            => new(_options);

        public Task<DisciplineTimelineDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDbContext());
    }
}
