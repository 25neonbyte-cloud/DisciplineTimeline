using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Core.Recurrence;
using DisciplineTimeline.Core.Repositories;

namespace DisciplineTimeline.Core.Services;

public sealed class RecurrenceService
{
    private readonly ITaskRepository _taskRepository;
    private readonly TimeProvider _timeProvider;

    public RecurrenceService(
        ITaskRepository taskRepository,
        TimeProvider timeProvider)
    {
        _taskRepository = taskRepository;
        _timeProvider = timeProvider;
    }

    public async Task<int> EnsureGeneratedThroughAsync(
        DateOnly throughDate,
        CancellationToken cancellationToken = default)
    {
        var roots = await _taskRepository.GetRecurrenceRootsAsync(cancellationToken);
        var generated = new List<TaskItem>();

        foreach (var root in roots)
        {
            foreach (var occurrenceDate in RecurrenceRuleFactory.EnumerateOccurrences(
                         root.OriginalPlannedDate,
                         throughDate,
                         root.RecurrenceRule))
            {
                if (await _taskRepository.RecurrenceOccurrenceExistsAsync(
                        root.Id,
                        occurrenceDate,
                        cancellationToken))
                {
                    continue;
                }

                generated.Add(new TaskItem
                {
                    Title = root.Title,
                    Description = root.Description,
                    CategoryId = root.CategoryId,
                    Priority = root.Priority,
                    CreatedAt = _timeProvider.GetLocalNow(),
                    OriginalPlannedDate = occurrenceDate,
                    CurrentPlannedDate = occurrenceDate,
                    PlannedStartTime = root.PlannedStartTime,
                    PlannedEndTime = root.PlannedEndTime,
                    Status = TaskState.Planned,
                    HasLateFlag = false,
                    HasRescheduledFlag = false,
                    RescheduleCount = 0,
                    HasCancellationFlag = false,
                    Origin = $"Recurrence:{root.Id}",
                    CycleId = root.CycleId,
                    RecurrenceRule = root.RecurrenceRule
                });
            }
        }

        if (generated.Count > 0)
        {
            await _taskRepository.AddManyAsync(generated, cancellationToken);
        }

        return generated.Count;
    }
}
