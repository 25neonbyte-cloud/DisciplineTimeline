namespace DisciplineTimeline.Models;

public sealed class TaskItem
{
    public long Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }

    public long CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;

    public TaskPriority Priority { get; init; } = TaskPriority.Normal;

    public DateTime CreatedAt { get; init; }
    public DateOnly OriginalPlannedDate { get; init; }
    public DateOnly CurrentPlannedDate { get; init; }

    public TimeOnly? PlannedStartTime { get; init; }
    public TimeOnly? PlannedEndTime { get; init; }

    public DateTime? ActualStartAt { get; init; }
    public DateTime? CompletedAt { get; init; }

    public TaskStatus Status { get; init; } = TaskStatus.Planned;

    public bool HasLateFlag { get; init; }
    public bool HasRescheduledFlag { get; init; }
    public int RescheduleCount { get; init; }
    public bool HasCancellationFlag { get; init; }

    public long? RecoveredFromTaskId { get; init; }
    public long? CycleId { get; init; }

    public string? RecurrenceRule { get; init; }
}
