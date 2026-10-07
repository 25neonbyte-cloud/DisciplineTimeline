using DisciplineTimeline.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DisciplineTimeline.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly IDbContextFactory<DisciplineTimelineDbContext> _dbContextFactory;
    private readonly TimeProvider _timeProvider;

    public DatabaseInitializer(
        IDbContextFactory<DisciplineTimelineDbContext> dbContextFactory,
        TimeProvider timeProvider)
    {
        _dbContextFactory = dbContextFactory;
        _timeProvider = timeProvider;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await db.Database.EnsureCreatedAsync(cancellationToken);

        // A inclusão do primeiro ciclo não altera o schema SQLite já utilizado.
        var start = new DateOnly(2026, 10, 6);
        var end = new DateOnly(2026, 10, 25);

        if (!await db.Cycles.AnyAsync(
                c => c.StartDate == start && c.EndDate == end,
                cancellationToken))
        {
            db.Cycles.Add(new PlanningCycle
            {
                Name = "Ciclo inicial — 06/10 a 25/10/2026",
                StartDate = start,
                EndDate = end,
                CreatedAt = _timeProvider.GetLocalNow()
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
