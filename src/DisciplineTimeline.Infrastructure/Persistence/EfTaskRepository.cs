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

    public async Task<TaskItem?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Tasks
            .AsNoTracking()
            .Include(x => x.Category)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<TaskItem> AddAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        task.Category = null;
        task.Cycle = null;
        task.RecoveredFromTask = null;

        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);

        return await db.Tasks
            .AsNoTracking()
            .Include(x => x.Category)
            .SingleAsync(x => x.Id == task.Id, cancellationToken);
    }

    public async Task UpdateAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        task.Category = null;
        task.Cycle = null;
        task.RecoveredFromTask = null;

        db.Tasks.Update(task);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        task.Category = null;
        task.Cycle = null;
        task.RecoveredFromTask = null;

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
    }
}
