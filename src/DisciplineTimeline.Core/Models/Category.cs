namespace DisciplineTimeline.Core.Models;

public sealed class Category
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public RecoveryPolicy RecoveryPolicy { get; set; }
    public bool IsSystemCategory { get; set; }
}
