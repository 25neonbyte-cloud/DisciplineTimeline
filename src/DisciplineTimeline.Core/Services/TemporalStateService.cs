using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Core.Repositories;
using DisciplineTimeline.Core.Time;

namespace DisciplineTimeline.Core.Services;

public sealed class TemporalStateService
{
    private readonly ITaskRepository _taskRepository;
    private readonly TaskTemporalEvaluator _temporalEvaluator;

    public TemporalStateService(
        ITaskRepository taskRepository,
        TaskTemporalEvaluator temporalEvaluator)
    {
        _taskRepository = taskRepository;
        _temporalEvaluator = temporalEvaluator;
    }

    public async Task<int> SynchronizeAsync(
        CancellationToken cancellationToken = default)
    {
        var openTasks = await _taskRepository.GetOpenAsync(cancellationToken);
        var changed = new List<TaskItem>();

        foreach (var task in openTasks)
        {
            var evaluation = _temporalEvaluator.Evaluate(task);
            var wasChanged = false;

            if (evaluation.IsLate && !task.HasLateFlag)
            {
                task.HasLateFlag = true;
                wasChanged = true;
            }

            if (evaluation.IsLost && task.Status != TaskState.Lost)
            {
                task.Status = TaskState.Lost;
                wasChanged = true;
            }

            if (wasChanged)
            {
                changed.Add(task);
            }
        }

        if (changed.Count > 0)
        {
            await _taskRepository.UpdateManyAsync(changed, cancellationToken);
        }

        return changed.Count;
    }
}
