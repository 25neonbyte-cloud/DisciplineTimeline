namespace DisciplineTimeline.Core.Models;

public sealed class TaskItem
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public long CategoryId { get; set; }
    public Category? Category { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Normal;

    public DateTimeOffset CreatedAt { get; set; }
    public DateOnly OriginalPlannedDate { get; set; }
    public DateOnly CurrentPlannedDate { get; set; }

    public TimeOnly? PlannedStartTime { get; set; }
    public TimeOnly? PlannedEndTime { get; set; }

    public DateTimeOffset? ActualStartAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Planned;

    public bool HasLateFlag { get; set; }
    public bool HasRescheduledFlag { get; set; }
    public int RescheduleCount { get; set; }
    public bool HasCancellationFlag { get; set; }

    public string Origin { get; set; } = "Manual";

    public long? RecoveredFromTaskId { get; set; }
    public TaskItem? RecoveredFromTask { get; set; }

    public long? CycleId { get; set; }
    public PlanningCycle? Cycle { get; set; }

    public string? RecurrenceRule { get; set; }
}
