using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DisciplineTimeline.Infrastructure.Persistence;

public sealed class EfCategoryRepository : ICategoryRepository
{
    private readonly IDbContextFactory<DisciplineTimelineDbContext> _dbContextFactory;

    public EfCategoryRepository(IDbContextFactory<DisciplineTimelineDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Categories
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
