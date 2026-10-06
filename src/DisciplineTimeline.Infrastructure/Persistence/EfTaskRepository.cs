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

    public async Task<IReadOnlyList<TaskItem>> GetOpenAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Tasks
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.Status == TaskState.Planned ||
                        x.Status == TaskState.InProgress)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>> GetRecurrenceRootsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Tasks
            .AsNoTracking()
            .Where(x => x.RecurrenceRule != null &&
                        x.Origin == "RecurrenceRoot")
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> RecurrenceOccurrenceExistsAsync(
        long recurrenceRootId,
        DateOnly occurrenceDate,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var origin = $"Recurrence:{recurrenceRootId}";

        return await db.Tasks.AnyAsync(
            x => x.Origin == origin &&
                 x.OriginalPlannedDate == occurrenceDate,
            cancellationToken);
    }

    public async Task<bool> RecoveryExistsAsync(
        long originalTaskId,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Tasks.AnyAsync(
            x => x.RecoveredFromTaskId == originalTaskId,
            cancellationToken);
    }

    public async Task<TaskItem> AddAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        ClearNavigation(task);

        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);

        return await db.Tasks
            .AsNoTracking()
            .Include(x => x.Category)
            .SingleAsync(x => x.Id == task.Id, cancellationToken);
    }

    public async Task AddManyAsync(
        IReadOnlyCollection<TaskItem> tasks,
        CancellationToken cancellationToken = default)
    {
        if (tasks.Count == 0)
        {
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        foreach (var task in tasks)
        {
            ClearNavigation(task);
        }

        db.Tasks.AddRange(tasks);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        ClearNavigation(task);

        db.Tasks.Update(task);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateManyAsync(
        IReadOnlyCollection<TaskItem> tasks,
        CancellationToken cancellationToken = default)
    {
        if (tasks.Count == 0)
        {
            return;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        foreach (var task in tasks)
        {
            ClearNavigation(task);
        }

        db.Tasks.UpdateRange(tasks);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        ClearNavigation(task);

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void ClearNavigation(TaskItem task)
    {
        task.Category = null;
        task.Cycle = null;
        task.RecoveredFromTask = null;
    }
}
