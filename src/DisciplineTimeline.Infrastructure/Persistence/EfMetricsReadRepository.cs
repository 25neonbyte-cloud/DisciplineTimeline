using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DisciplineTimeline.Infrastructure.Persistence;

public sealed class EfMetricsReadRepository : IMetricsReadRepository
{
    private readonly IDbContextFactory<DisciplineTimelineDbContext> _contextFactory;

    public EfMetricsReadRepository(
        IDbContextFactory<DisciplineTimelineDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<TaskItem>> GetReportTasksAsync(
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Tasks
            .AsNoTracking()
            .Where(t =>
                (t.OriginalPlannedDate >= start && t.OriginalPlannedDate <= end) ||
                (t.CurrentPlannedDate >= start && t.CurrentPlannedDate <= end) ||
                (t.RecoveredFromTaskId != null &&
                 t.RecoveredFromTask != null &&
                 ((t.RecoveredFromTask.OriginalPlannedDate >= start &&
                   t.RecoveredFromTask.OriginalPlannedDate <= end) ||
                  (t.RecoveredFromTask.CurrentPlannedDate >= start &&
                   t.RecoveredFromTask.CurrentPlannedDate <= end))))
            .OrderBy(t => t.OriginalPlannedDate)
            .ThenBy(t => t.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PlanningCycle>> GetCyclesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Cycles
            .AsNoTracking()
            .OrderBy(c => c.StartDate)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }
}
