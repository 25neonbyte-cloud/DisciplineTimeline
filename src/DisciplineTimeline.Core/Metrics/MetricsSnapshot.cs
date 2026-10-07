using DisciplineTimeline.Core.Models;

namespace DisciplineTimeline.Core.Metrics;

/// <summary>
/// Leitura histórica imutável para cálculos determinísticos, sem acesso direto a EF/WPF.
/// Planejamento diário acompanha a data operacional; relatório de período ancora
/// o denominador na data originalmente prevista, impedindo apagamento por reagendamento.
/// </summary>
public sealed class MetricsSnapshot
{
    private readonly IReadOnlyList<TaskItem> _tasks;
    private readonly HashSet<long> _recoveredOriginalIds;

    public DateOnly Today { get; }

    public MetricsSnapshot(IReadOnlyList<TaskItem> tasks, DateOnly today)
    {
        _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
        Today = today;

        _recoveredOriginalIds = tasks
            .Where(t => t.RecoveredFromTaskId.HasValue && t.Status == TaskState.Completed)
            .Select(t => t.RecoveredFromTaskId!.Value)
            .ToHashSet();
    }

    public DailyMetrics ForDay(DateOnly date)
    {
        var planned = _tasks
            .Where(t => t.RecoveredFromTaskId is null && t.CurrentPlannedDate == date)
            .ToArray();

        var originallyPlanned = _tasks
            .Where(t => t.RecoveredFromTaskId is null && t.OriginalPlannedDate == date)
            .ToArray();

        var completed = planned.Count(t => t.Status == TaskState.Completed &&
            CompletedOn(t, date));

        var bonuses = _tasks.Count(t =>
            t.RecoveredFromTaskId.HasValue &&
            t.Status == TaskState.Completed &&
            t.CurrentPlannedDate == date);

        return new DailyMetrics(
            date,
            planned.Length,
            completed,
            planned.Count(t => t.Status == TaskState.Lost),
            planned.Count(t => t.HasLateFlag),
            planned.Count(t => t.Status == TaskState.Cancelled),
            planned.Count(t => t.HasRescheduledFlag),
            planned.Sum(t => t.RescheduleCount),
            originallyPlanned.Length,
            originallyPlanned.Count(t => t.CurrentPlannedDate != date),
            bonuses,
            Percent(completed, planned.Length),
            Percent(completed, planned.Length));
    }

    public PeriodMetrics ForPeriod(DateOnly start, DateOnly end)
    {
        if (end < start)
        {
            throw new ArgumentException("O fim do período não pode anteceder o início.");
        }

        // A referência do período é a data ORIGINAL, não a data reagendada.
        var plan = _tasks.Where(t =>
            t.RecoveredFromTaskId is null &&
            t.OriginalPlannedDate >= start &&
            t.OriginalPlannedDate <= end).ToArray();

        var delivered = plan.Count(t =>
            t.Status == TaskState.Completed || _recoveredOriginalIds.Contains(t.Id));

        var onOperationalDate = plan.Count(t =>
            t.Status == TaskState.Completed &&
            CompletedOn(t, t.CurrentPlannedDate));

        var bonusWithinPeriod = _tasks.Count(t =>
            t.RecoveredFromTaskId.HasValue &&
            t.Status == TaskState.Completed &&
            t.CurrentPlannedDate >= start &&
            t.CurrentPlannedDate <= end);

        var full = 0;
        var partial = 0;
        var zero = 0;

        // Dias futuros não devem ser classificados como fracasso.
        var measuredUntil = end < Today ? end : Today;
        for (var day = start; day <= measuredUntil; day = day.AddDays(1))
        {
            var metrics = ForDay(day);
            if (metrics.Planned == 0)
            {
                continue;
            }

            if (metrics.Completed == metrics.Planned)
            {
                full++;
            }
            else if (metrics.Completed == 0)
            {
                zero++;
            }
            else
            {
                partial++;
            }
        }

        var changes = plan.Sum(t => t.RescheduleCount);

        return new PeriodMetrics(
            start,
            end,
            plan.Length,
            delivered,
            onOperationalDate,
            plan.Count(t => t.Status == TaskState.Lost),
            plan.Count(t => t.HasLateFlag),
            plan.Count(t => t.Status == TaskState.Cancelled),
            plan.Count(t => _recoveredOriginalIds.Contains(t.Id)),
            bonusWithinPeriod,
            plan.Count(t => t.HasRescheduledFlag),
            changes,
            plan.Length == 0 ? 0 : (double)changes / plan.Length,
            full,
            partial,
            zero,
            Percent(delivered, plan.Length),
            Percent(onOperationalDate, plan.Length));
    }

    public static double? Percent(int numerator, int denominator)
        => denominator == 0 ? null :
            Math.Clamp(100d * numerator / denominator, 0d, 100d);

    private static bool CompletedOn(TaskItem task, DateOnly date)
        => task.CompletedAt.HasValue &&
           DateOnly.FromDateTime(task.CompletedAt.Value.Date) == date;
}
