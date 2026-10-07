using DisciplineTimeline.Core.Models;

namespace DisciplineTimeline.Core.Requests;

public sealed record CreateScheduledTaskRequest(
    string Title,
    string? Description,
    long CategoryId,
    TaskPriority Priority,
    DateOnly PlannedDate,
    TimeOnly? PlannedStartTime,
    TimeOnly? PlannedEndTime,
    long? CycleId = null,
    RecurrenceKind Recurrence = RecurrenceKind.None);
