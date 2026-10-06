using DisciplineTimeline.Core.Models;

namespace DisciplineTimeline.Core.Repositories;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetForDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskItem>> GetOpenAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskItem>> GetRecurrenceRootsAsync(
        CancellationToken cancellationToken = default);

    Task<bool> RecurrenceOccurrenceExistsAsync(
        long recurrenceRootId,
        DateOnly occurrenceDate,
        CancellationToken cancellationToken = default);

    Task<bool> RecoveryExistsAsync(
        long originalTaskId,
        CancellationToken cancellationToken = default);

    Task<TaskItem> AddAsync(
        TaskItem task,
        CancellationToken cancellationToken = default);

    Task AddManyAsync(
        IReadOnlyCollection<TaskItem> tasks,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        TaskItem task,
        CancellationToken cancellationToken = default);

    Task UpdateManyAsync(
        IReadOnlyCollection<TaskItem> tasks,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        TaskItem task,
        CancellationToken cancellationToken = default);
}
