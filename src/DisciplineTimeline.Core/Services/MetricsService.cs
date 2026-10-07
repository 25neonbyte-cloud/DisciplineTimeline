using DisciplineTimeline.Core.Metrics;
using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Core.Repositories;

namespace DisciplineTimeline.Core.Services;

public sealed class MetricsService
{
    private readonly IMetricsReadRepository _readRepository;
    private readonly TimeProvider _timeProvider;

    public MetricsService(
        IMetricsReadRepository readRepository,
        TimeProvider timeProvider)
    {
        _readRepository = readRepository;
        _timeProvider = timeProvider;
    }

    public Task<IReadOnlyList<PlanningCycle>> GetCyclesAsync(
        CancellationToken cancellationToken = default)
        => _readRepository.GetCyclesAsync(cancellationToken);

    public async Task<MetricsSnapshot> GetSnapshotAsync(
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default)
    {
        if (end < start)
        {
            throw new ArgumentException("O período informado é inválido.");
        }

        var tasks = await _readRepository.GetReportTasksAsync(
            start, end, cancellationToken);

        var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
        return new MetricsSnapshot(tasks, today);
    }
}
