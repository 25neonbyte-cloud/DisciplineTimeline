using DisciplineTimeline.Core.Models;

namespace DisciplineTimeline.Core.Time;

public sealed class TaskTemporalEvaluator
{
    private readonly TimeProvider _timeProvider;

    public TaskTemporalEvaluator(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public TaskTemporalEvaluation Evaluate(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        var now = _timeProvider.GetLocalNow();
        var today = DateOnly.FromDateTime(now.DateTime);
        var currentTime = TimeOnly.FromDateTime(now.DateTime);

        if (task.Status is TaskState.Completed or TaskState.Cancelled or TaskState.Lost)
        {
            return new TaskTemporalEvaluation(
                now,
                task.HasLateFlag,
                task.Status == TaskState.Lost,
                task.Status);
        }

        var isLost = today > task.CurrentPlannedDate;
        var isLate = task.HasLateFlag;

        if (!isLost &&
            today == task.CurrentPlannedDate &&
            task.PlannedEndTime is { } endTime &&
            currentTime >= endTime)
        {
            isLate = true;
        }

        return new TaskTemporalEvaluation(
            now,
            isLate,
            isLost,
            isLost ? TaskState.Lost : task.Status);
    }
}
