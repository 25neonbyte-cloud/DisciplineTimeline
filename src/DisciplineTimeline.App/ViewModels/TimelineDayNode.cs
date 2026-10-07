using DisciplineTimeline.Core.Metrics;

namespace DisciplineTimeline.App.ViewModels;

public sealed class TimelineDayNode
{
    public DateTime Date { get; }
    public string WeekDay { get; }
    public string DayLabel { get; }
    public string StatusLabel { get; }
    public bool IsSelected { get; }
    public bool IsToday { get; }
    public bool IsPast { get; }

    public TimelineDayNode(DailyMetrics metrics, DateOnly selected, DateOnly today)
    {
        var date = metrics.Date;
        Date = date.ToDateTime(TimeOnly.MinValue);
        WeekDay = Date.ToString("ddd", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
        DayLabel = Date.ToString("dd/MM");
        IsSelected = date == selected;
        IsToday = date == today;
        IsPast = date < today;

        StatusLabel = metrics.Lost > 0 && IsPast
            ? $"Perdidas: {metrics.Lost}"
            : metrics.Planned > 0
                ? $"{metrics.Completed}/{metrics.Planned} concluídas"
                : metrics.MovedAwayFromOriginalDate > 0
                    ? $"Movidas: {metrics.MovedAwayFromOriginalDate}"
                    : metrics.RecoveredBonuses > 0
                        ? $"+{metrics.RecoveredBonuses} bônus"
                        : "Sem demandas";
    }
}
