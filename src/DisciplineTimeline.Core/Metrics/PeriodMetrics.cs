namespace DisciplineTimeline.Core.Metrics;

public sealed record PeriodMetrics(
    DateOnly Start,
    DateOnly End,
    int Planned,
    int Delivered,
    int OnOperationalDate,
    int Lost,
    int Late,
    int Cancelled,
    int Recovered,
    int RecoveredBonusesWithinPeriod,
    int Rescheduled,
    int RescheduleChanges,
    double AverageRescheduleChanges,
    int FullDays,
    int PartialDays,
    int ZeroDays,
    double? DeliveryPercent,
    double? ConsistencyPercent);
