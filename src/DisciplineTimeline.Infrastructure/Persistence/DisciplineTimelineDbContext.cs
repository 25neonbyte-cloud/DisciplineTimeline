using DisciplineTimeline.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DisciplineTimeline.Infrastructure.Persistence;

public sealed class DisciplineTimelineDbContext : DbContext
{
    public DisciplineTimelineDbContext(DbContextOptions<DisciplineTimelineDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<PlanningCycle> Cycles => Set<PlanningCycle>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var category = modelBuilder.Entity<Category>();
        category.ToTable("Categories");
        category.HasKey(x => x.Id);
        category.Property(x => x.Name).IsRequired().HasMaxLength(100);
        category.HasIndex(x => x.Name).IsUnique();
        category.Property(x => x.RecoveryPolicy).HasConversion<string>().HasMaxLength(32);
        category.HasData(
            new Category
            {
                Id = 1,
                Name = "Miscelânia",
                RecoveryPolicy = RecoveryPolicy.Recoverable,
                IsSystemCategory = true
            },
            new Category
            {
                Id = 2,
                Name = "Exercícios",
                RecoveryPolicy = RecoveryPolicy.NonRecoverable,
                IsSystemCategory = true
            });

        var cycle = modelBuilder.Entity<PlanningCycle>();
        cycle.ToTable("Cycles");
        cycle.HasKey(x => x.Id);
        cycle.Property(x => x.Name).IsRequired().HasMaxLength(160);

        var task = modelBuilder.Entity<TaskItem>();
        task.ToTable("Tasks");
        task.HasKey(x => x.Id);
        task.Property(x => x.Title).IsRequired().HasMaxLength(240);
        task.Property(x => x.Description).HasMaxLength(4000);
        task.Property(x => x.Origin).IsRequired().HasMaxLength(64);
        task.Property(x => x.Priority).HasConversion<int>();
        task.Property(x => x.Status).HasConversion<int>();

        task.HasIndex(x => x.CurrentPlannedDate);
        task.HasIndex(x => x.OriginalPlannedDate);
        task.HasIndex(x => x.Status);

        task.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        task.HasOne(x => x.Cycle)
            .WithMany()
            .HasForeignKey(x => x.CycleId)
            .OnDelete(DeleteBehavior.SetNull);

        task.HasOne(x => x.RecoveredFromTask)
            .WithMany()
            .HasForeignKey(x => x.RecoveredFromTaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
