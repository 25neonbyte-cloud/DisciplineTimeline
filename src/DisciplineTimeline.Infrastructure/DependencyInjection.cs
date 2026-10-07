using DisciplineTimeline.Core.Repositories;
using DisciplineTimeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DisciplineTimeline.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDisciplineTimelineInfrastructure(
        this IServiceCollection services,
        string? databasePath = null)
    {
        databasePath ??= GetDefaultDatabasePath();

        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        services.AddDbContextFactory<DisciplineTimelineDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        services.AddSingleton<ITaskRepository, EfTaskRepository>();
        services.AddSingleton<ICategoryRepository, EfCategoryRepository>();
        services.AddSingleton<IMetricsReadRepository, EfMetricsReadRepository>();
        services.AddSingleton<DatabaseInitializer>();

        return services;
    }

    public static string GetDefaultDatabasePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "DisciplineTimeline", "discipline-timeline.db");
    }
}
