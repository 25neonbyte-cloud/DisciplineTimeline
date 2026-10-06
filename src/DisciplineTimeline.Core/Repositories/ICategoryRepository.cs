using DisciplineTimeline.Core.Models;

namespace DisciplineTimeline.Core.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);
}
