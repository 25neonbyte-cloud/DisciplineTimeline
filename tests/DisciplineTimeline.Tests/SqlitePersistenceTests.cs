using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DisciplineTimeline.Tests;

public sealed class SqlitePersistenceTests
{
    [Fact]
    public async Task EnsureCreated_SeedsInitialCategories()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"discipline-timeline-tests-{Guid.NewGuid():N}.db");

        try
        {
            var options = new DbContextOptionsBuilder<DisciplineTimelineDbContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options;

            await using var db = new DisciplineTimelineDbContext(options);
            await db.Database.EnsureCreatedAsync();

            var categories = await db.Categories
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .ToListAsync();

            Assert.Equal(2, categories.Count);
            Assert.Equal("Miscelânia", categories[0].Name);
            Assert.Equal(RecoveryPolicy.Recoverable, categories[0].RecoveryPolicy);
            Assert.Equal("Exercícios", categories[1].Name);
            Assert.Equal(RecoveryPolicy.NonRecoverable, categories[1].RecoveryPolicy);
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}
