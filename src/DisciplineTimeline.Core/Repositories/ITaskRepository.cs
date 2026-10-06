using DisciplineTimeline.Core.Models;

namespace DisciplineTimeline.Core.Repositories;

public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetForDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default);
}
