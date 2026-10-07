using DisciplineTimeline.Core.Models;

namespace DisciplineTimeline.Core.Repositories;

public interface IMetricsReadRepository
{
    // Inclui tarefas vinculadas por planejamento original, operacional ou recuperação,
    // sem restringir a conclusão posterior ao período original.
    Task<IReadOnlyList<TaskItem>> GetReportTasksAsync(
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlanningCycle>> GetCyclesAsync(
        CancellationToken cancellationToken = default);
}
