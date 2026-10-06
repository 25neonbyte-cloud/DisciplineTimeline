namespace DisciplineTimeline.Core.Models;

public sealed class PlanningCycle
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
