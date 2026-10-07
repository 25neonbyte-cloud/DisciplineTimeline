using DisciplineTimeline.Core.Metrics;
using DisciplineTimeline.Core.Models;
using Xunit;

namespace DisciplineTimeline.Tests;

public sealed class MetricsSnapshotTests
{
    private static readonly DateOnly Day8 = new(2026, 10, 8);
    private static readonly DateOnly Day10 = new(2026, 10, 10);

    [Fact]
    public void NoPlannedTasks_ShowsUndefinedPercentage_NotFalseHundred()
    {
        var day = new MetricsSnapshot([], Day10).ForDay(Day8);

        Assert.Null(day.ExecutionPercent);
        Assert.Null(day.ConsistencyPercent);
        Assert.Equal(0, day.Planned);
    }

    [Fact]
    public void LateCompletionOnSameDay_CountsAsConsistentButPreservesLateFlag()
    {
        var task = Task(1, Day8, TaskState.Completed, completed: At(Day8, 19));
        task.HasLateFlag = true;

        var day = new MetricsSnapshot([task], Day10).ForDay(Day8);

        Assert.Equal(1, day.Late);
        Assert.Equal(1, day.Completed);
        Assert.Equal(100, day.ConsistencyPercent);
        Assert.Equal(100, day.ExecutionPercent);
    }

    [Fact]
    public void Recovery_DoesNotIncreaseDailyDenominatorOrPercentage()
    {
        var lost = Task(1, Day8, TaskState.Lost);
        var normal = Task(2, Day10, TaskState.Completed, completed: At(Day10, 15));
        var bonus = Task(3, Day10, TaskState.Completed, completed: At(Day10, 18));
        bonus.RecoveredFromTaskId = lost.Id;
        bonus.Origin = "RecoveredBonus";

        var report = new MetricsSnapshot([lost, normal, bonus], Day10);
        var day = report.ForDay(Day10);

        Assert.Equal(1, day.Planned);
        Assert.Equal(1, day.Completed);
        Assert.Equal(1, day.RecoveredBonuses);
        Assert.Equal(100, day.ExecutionPercent);
        Assert.Equal(1, report.ForDay(Day8).Lost);
    }

    [Fact]
    public void Reschedule_PreservesOriginalDayHistoryAndOperationalDay()
    {
        var changed = Task(1, Day8, TaskState.Completed, completed: At(Day10, 13));
        changed.CurrentPlannedDate = Day10;
        changed.HasRescheduledFlag = true;
        changed.RescheduleCount = 2;

        var snapshot = new MetricsSnapshot([changed], Day10);

        Assert.Equal(0, snapshot.ForDay(Day8).Planned);
        Assert.Equal(1, snapshot.ForDay(Day8).OriginallyPlanned);
        Assert.Equal(1, snapshot.ForDay(Day8).MovedAwayFromOriginalDate);
        Assert.Equal(1, snapshot.ForDay(Day10).Planned);
        Assert.Equal(2, snapshot.ForDay(Day10).RescheduleChanges);

        var period = snapshot.ForPeriod(Day8, Day8);
        Assert.Equal(1, period.Planned);
        Assert.Equal(1, period.Delivered);
        Assert.Equal(1, period.OnOperationalDate);
        Assert.Equal(1, period.Rescheduled);
        Assert.Equal(2, period.RescheduleChanges);
    }

    [Fact]
    public void RecoveryAfterPeriod_UpdatesDeliveryButNotConsistency()
    {
        var lost = Task(1, Day8, TaskState.Lost);
        var recovered = Task(2, Day10, TaskState.Completed, completed: At(Day10, 10));
        recovered.RecoveredFromTaskId = lost.Id;
        recovered.Origin = "RecoveredBonus";

        var metrics = new MetricsSnapshot([lost, recovered], Day10).ForPeriod(Day8, Day8);

        Assert.Equal(1, metrics.Planned);
        Assert.Equal(1, metrics.Delivered);
        Assert.Equal(1, metrics.Recovered);
        Assert.Equal(100, metrics.DeliveryPercent);
        Assert.Equal(0, metrics.ConsistencyPercent);
        Assert.Equal(0, metrics.RecoveredBonusesWithinPeriod);
        Assert.Equal(1, metrics.Lost);
    }

    [Fact]
    public void CancelledTask_RemainsInHistoricalPlan_NotCountedAsDelivered()
    {
        var task = Task(1, Day8, TaskState.Cancelled);
        task.HasCancellationFlag = true;

        var period = new MetricsSnapshot([task], Day10).ForPeriod(Day8, Day10);

        Assert.Equal(1, period.Planned);
        Assert.Equal(0, period.Delivered);
        Assert.Equal(1, period.Cancelled);
        Assert.Equal(0, period.DeliveryPercent);
    }

    [Fact]
    public void PlannedDays_ClassifyFullPartialZeroWithoutCountingFutureDays()
    {
        var day8Task = Task(1, Day8, TaskState.Completed, At(Day8, 11));
        var day9TaskA = Task(2, new DateOnly(2026, 10, 9), TaskState.Completed, At(new DateOnly(2026, 10, 9), 11));
        var day9TaskB = Task(3, new DateOnly(2026, 10, 9), TaskState.Lost);
        var day10Task = Task(4, Day10, TaskState.Planned);
        var futureTask = Task(5, new DateOnly(2026, 10, 11), TaskState.Planned);

        var period = new MetricsSnapshot(
            [day8Task, day9TaskA, day9TaskB, day10Task, futureTask],
            Day10)
            .ForPeriod(Day8, new DateOnly(2026, 10, 11));

        Assert.Equal(1, period.FullDays);
        Assert.Equal(1, period.PartialDays);
        Assert.Equal(1, period.ZeroDays);
    }

    [Fact]
    public void EmptyPeriod_ContainsNoZeroDays()
    {
        var period = new MetricsSnapshot([], Day10).ForPeriod(Day8, Day10);

        Assert.Equal(0, period.FullDays);
        Assert.Equal(0, period.PartialDays);
        Assert.Equal(0, period.ZeroDays);
        Assert.Null(period.DeliveryPercent);
    }

    private static TaskItem Task(
        long id,
        DateOnly day,
        TaskState status,
        DateTimeOffset? completed = null)
        => new()
        {
            Id = id,
            Title = $"Tarefa {id}",
            CategoryId = 1,
            OriginalPlannedDate = day,
            CurrentPlannedDate = day,
            Status = status,
            CompletedAt = completed
        };

    private static DateTimeOffset At(DateOnly day, int hour)
        => new(day.Year, day.Month, day.Day, hour, 0, 0, TimeSpan.FromHours(-3));
}
