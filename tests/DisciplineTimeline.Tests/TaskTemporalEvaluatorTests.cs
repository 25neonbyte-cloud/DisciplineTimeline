using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Core.Time;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace DisciplineTimeline.Tests;

public sealed class TaskTemporalEvaluatorTests
{
    [Fact]
    public void StartOnlyTask_IsNotLate_WhenStartedLaterOnSameDay()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 6, 16, 0, 0, TimeSpan.Zero));
        var evaluator = new TaskTemporalEvaluator(clock);

        var task = NewTask(new DateOnly(2026, 10, 6));
        task.PlannedStartTime = new TimeOnly(14, 0);

        var result = evaluator.Evaluate(task);

        Assert.False(result.IsLate);
        Assert.False(result.IsLost);
        Assert.Equal(TaskState.Planned, result.SuggestedStatus);
    }

    [Fact]
    public void TaskWithEndTime_BecomesLate_AtDeadline()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 6, 16, 0, 0, TimeSpan.Zero));
        var evaluator = new TaskTemporalEvaluator(clock);

        var task = NewTask(new DateOnly(2026, 10, 6));
        task.PlannedStartTime = new TimeOnly(14, 0);
        task.PlannedEndTime = new TimeOnly(16, 0);

        var result = evaluator.Evaluate(task);

        Assert.True(result.IsLate);
        Assert.False(result.IsLost);
    }

    [Fact]
    public void PendingTask_BecomesLost_AfterPlannedDayEnds()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 7, 0, 0, 0, TimeSpan.Zero));
        var evaluator = new TaskTemporalEvaluator(clock);

        var task = NewTask(new DateOnly(2026, 10, 6));

        var result = evaluator.Evaluate(task);

        Assert.True(result.IsLost);
        Assert.Equal(TaskState.Lost, result.SuggestedStatus);
    }

    [Fact]
    public void CompletedTask_PreservesHistoricalLateFlag()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 7, 10, 0, 0, TimeSpan.Zero));
        var evaluator = new TaskTemporalEvaluator(clock);

        var task = NewTask(new DateOnly(2026, 10, 6));
        task.Status = TaskState.Completed;
        task.HasLateFlag = true;

        var result = evaluator.Evaluate(task);

        Assert.True(result.IsLate);
        Assert.False(result.IsLost);
        Assert.Equal(TaskState.Completed, result.SuggestedStatus);
    }

    private static TaskItem NewTask(DateOnly date) => new()
    {
        Title = "Teste",
        CategoryId = 1,
        CreatedAt = new DateTimeOffset(2026, 10, 6, 9, 0, 0, TimeSpan.Zero),
        OriginalPlannedDate = date,
        CurrentPlannedDate = date,
        Status = TaskState.Planned
    };
}
