namespace DisciplineTimeline.Core.Metrics;

public sealed record DailyMetrics(
    DateOnly Date,
    int Planned,
    int Completed,
    int Lost,
    int Late,
    int Cancelled,
    int Rescheduled,
    int RescheduleChanges,
    int OriginallyPlanned,
    int MovedAwayFromOriginalDate,
    int RecoveredBonuses,
    double? ExecutionPercent,
    double? ConsistencyPercent);
