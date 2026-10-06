using DisciplineTimeline.Core.Models;

namespace DisciplineTimeline.Core.Requests;

public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    long CategoryId,
    TaskPriority Priority,
    TimeOnly? PlannedStartTime,
    TimeOnly? PlannedEndTime);
