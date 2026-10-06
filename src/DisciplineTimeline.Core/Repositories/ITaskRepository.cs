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

    Task<TaskItem> AddAsync(
        TaskItem task,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        TaskItem task,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        TaskItem task,
        CancellationToken cancellationToken = default);
}
