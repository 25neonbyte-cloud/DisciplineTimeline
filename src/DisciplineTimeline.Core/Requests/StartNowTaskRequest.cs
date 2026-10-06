using DisciplineTimeline.Core.Models;

namespace DisciplineTimeline.Core.Requests;

public sealed record StartNowTaskRequest(
    string Title,
    string? Description,
    long CategoryId,
    TaskPriority Priority,
    TimeOnly? PlannedEndTime,
    long? CycleId = null);
