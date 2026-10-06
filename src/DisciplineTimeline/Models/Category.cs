namespace DisciplineTimeline.Models;

public sealed class Category
{
    public long Id { get; init; }
    public required string Name { get; init; }
    public required string RecoveryPolicy { get; init; }
    public bool IsSystemCategory { get; init; }
}
