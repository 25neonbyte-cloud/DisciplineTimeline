using DisciplineTimeline.Core.Exceptions;
using DisciplineTimeline.Core.Models;

namespace DisciplineTimeline.Core.Recurrence;

public static class RecurrenceRuleFactory
{
    public const string DailyRule = "FREQ=DAILY";
    public const string WeeklyRule = "FREQ=WEEKLY";

    public static string? ToRule(RecurrenceKind kind)
        => kind switch
        {
            RecurrenceKind.None => null,
            RecurrenceKind.Daily => DailyRule,
            RecurrenceKind.Weekly => WeeklyRule,
            _ => throw new DomainValidationException("Recorrência não suportada.")
        };

    public static RecurrenceKind Parse(string? rule)
        => rule switch
        {
            null or "" => RecurrenceKind.None,
            DailyRule => RecurrenceKind.Daily,
            WeeklyRule => RecurrenceKind.Weekly,
            _ => throw new DomainValidationException(
                $"Regra de recorrência não suportada: {rule}.")
        };

    public static IEnumerable<DateOnly> EnumerateOccurrences(
        DateOnly anchorDate,
        DateOnly throughDate,
        string? rule)
    {
        var kind = Parse(rule);

        if (kind == RecurrenceKind.None || throughDate <= anchorDate)
        {
            yield break;
        }

        var stepDays = kind == RecurrenceKind.Daily ? 1 : 7;
        var current = anchorDate.AddDays(stepDays);

        while (current <= throughDate)
        {
            yield return current;
            current = current.AddDays(stepDays);
        }
    }
}
