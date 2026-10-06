using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DisciplineTimeline.Infrastructure.Persistence;

public sealed class EfTaskRepository : ITaskRepository
{
    private readonly IDbContextFactory<DisciplineTimelineDbContext> _dbContextFactory;

    public EfTaskRepository(IDbContextFactory<DisciplineTimelineDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<TaskItem>> GetForDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Tasks
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.CurrentPlannedDate == date)
            .OrderBy(x => x.PlannedStartTime == null)
            .ThenBy(x => x.PlannedStartTime)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}
