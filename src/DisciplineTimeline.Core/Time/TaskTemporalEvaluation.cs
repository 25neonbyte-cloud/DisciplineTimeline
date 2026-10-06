using DisciplineTimeline.Core.Models;

namespace DisciplineTimeline.Core.Time;

public sealed record TaskTemporalEvaluation(
    DateTimeOffset EvaluatedAt,
    bool IsLate,
    bool IsLost,
    TaskStatus SuggestedStatus);
