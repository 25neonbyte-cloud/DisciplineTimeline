using DisciplineTimeline.Core.Exceptions;
using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Core.Repositories;
using DisciplineTimeline.Core.Requests;
using DisciplineTimeline.Core.Services;
using DisciplineTimeline.Core.Time;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace DisciplineTimeline.Tests;

public sealed class TaskServiceTests
{
    private static readonly DateTimeOffset InitialNow =
        new(2026, 10, 6, 14, 30, 15, TimeSpan.Zero);

    [Fact]
    public async Task TemporalMaintenance_PersistsLateAndLostState()
    {
        var fixture = CreateFixture();
        var late = await fixture.Service.CreateScheduledAsync(
            NewScheduled(
                "Prazo",
                1,
                new DateOnly(2026, 10, 6),
                new TimeOnly(13, 0),
                new TimeOnly(14, 0)));

        var old = fixture.Repository.Seed(new TaskItem
        {
            Title = "Ontem",
            CategoryId = 1,
            CreatedAt = InitialNow.AddDays(-1),
            OriginalPlannedDate = new DateOnly(2026, 10, 5),
            CurrentPlannedDate = new DateOnly(2026, 10, 5),
            Status = TaskState.Planned,
            Origin = "Manual"
        });

        await fixture.Service.RunMaintenanceAsync();

        Assert.True((await fixture.Repository.GetByIdAsync(late.Id))!.HasLateFlag);
        Assert.Equal(TaskState.Lost, (await fixture.Repository.GetByIdAsync(old.Id))!.Status);
    }

    [Fact]
    public async Task Reschedule_PreservesOriginalDateAndIncrementsHistory()
    {
        var fixture = CreateFixture();
        var created = await fixture.Service.CreateScheduledAsync(
            NewScheduled("Mover", 1, new DateOnly(2026, 10, 8)));

        var rescheduled = await fixture.Service.RescheduleAsync(
            created.Id,
            new DateOnly(2026, 10, 10));

        Assert.Equal(new DateOnly(2026, 10, 8), rescheduled.OriginalPlannedDate);
        Assert.Equal(new DateOnly(2026, 10, 10), rescheduled.CurrentPlannedDate);
        Assert.True(rescheduled.HasRescheduledFlag);
        Assert.Equal(1, rescheduled.RescheduleCount);
    }

    [Fact]
    public async Task Reschedule_RejectsLateTask()
    {
        var fixture = CreateFixture();
        var created = await fixture.Service.CreateScheduledAsync(
            NewScheduled(
                "Vencida",
                1,
                new DateOnly(2026, 10, 6),
                new TimeOnly(13, 0),
                new TimeOnly(14, 0)));

        await Assert.ThrowsAsync<DomainValidationException>(
            () => fixture.Service.RescheduleAsync(
                created.Id,
                new DateOnly(2026, 10, 7)));
    }

    [Fact]
    public async Task Cancel_PreservesExistingLateCondition()
    {
        var fixture = CreateFixture();
        var created = await fixture.Service.CreateScheduledAsync(
            NewScheduled(
                "Cancelar",
                1,
                new DateOnly(2026, 10, 6),
                new TimeOnly(13, 0),
                new TimeOnly(14, 0)));

        var cancelled = await fixture.Service.CancelAsync(created.Id);

        Assert.Equal(TaskState.Cancelled, cancelled.Status);
        Assert.True(cancelled.HasCancellationFlag);
        Assert.True(cancelled.HasLateFlag);
    }

    [Fact]
    public async Task Recover_CreatesSeparateCompletedBonus_AndKeepsOriginalLost()
    {
        var fixture = CreateFixture();
        var original = fixture.Repository.Seed(new TaskItem
        {
            Title = "Perdida",
            CategoryId = 1,
            Category = fixture.Categories[0],
            CreatedAt = InitialNow.AddDays(-1),
            OriginalPlannedDate = new DateOnly(2026, 10, 5),
            CurrentPlannedDate = new DateOnly(2026, 10, 5),
            Status = TaskState.Lost,
            Origin = "Manual"
        });

        var recovered = await fixture.Service.RecoverAsync(original.Id);

        Assert.Equal(TaskState.Lost, original.Status);
        Assert.Equal(TaskState.Completed, recovered.Status);
        Assert.Equal(original.Id, recovered.RecoveredFromTaskId);
        Assert.Equal("RecoveredBonus", recovered.Origin);
        Assert.Equal(new DateOnly(2026, 10, 6), recovered.CurrentPlannedDate);
    }

    [Fact]
    public async Task Recover_RejectsLostExercise()
    {
        var fixture = CreateFixture();
        var exercise = fixture.Repository.Seed(new TaskItem
        {
            Title = "Exercício",
            CategoryId = 2,
            Category = fixture.Categories[1],
            CreatedAt = InitialNow.AddDays(-1),
            OriginalPlannedDate = new DateOnly(2026, 10, 5),
            CurrentPlannedDate = new DateOnly(2026, 10, 5),
            Status = TaskState.Lost,
            Origin = "Manual"
        });

        await Assert.ThrowsAsync<DomainValidationException>(
            () => fixture.Service.RecoverAsync(exercise.Id));
    }

    [Fact]
    public async Task Recover_RejectsDuplicateRecovery()
    {
        var fixture = CreateFixture();
        var original = fixture.Repository.Seed(new TaskItem
        {
            Title = "Perdida",
            CategoryId = 1,
            Category = fixture.Categories[0],
            CreatedAt = InitialNow.AddDays(-1),
            OriginalPlannedDate = new DateOnly(2026, 10, 5),
            CurrentPlannedDate = new DateOnly(2026, 10, 5),
            Status = TaskState.Lost,
            Origin = "Manual"
        });

        await fixture.Service.RecoverAsync(original.Id);

        await Assert.ThrowsAsync<DomainValidationException>(
            () => fixture.Service.RecoverAsync(original.Id));
    }

    [Fact]
    public async Task DailyRecurrence_GeneratesIndependentOccurrencesThroughRequestedDate()
    {
        var fixture = CreateFixture();
        var root = await fixture.Service.CreateScheduledAsync(
            NewScheduled(
                "Diária",
                1,
                new DateOnly(2026, 10, 6),
                recurrence: RecurrenceKind.Daily));

        var day8 = await fixture.Service.GetForDateAsync(new DateOnly(2026, 10, 8));
        var occurrence = Assert.Single(day8);

        Assert.NotEqual(root.Id, occurrence.Id);
        Assert.Equal(root.Title, occurrence.Title);
        Assert.Equal(new DateOnly(2026, 10, 8), occurrence.OriginalPlannedDate);
        Assert.Equal(new DateOnly(2026, 10, 8), occurrence.CurrentPlannedDate);
        Assert.Equal($"Recurrence:{root.Id}", occurrence.Origin);
        Assert.Equal("FREQ=DAILY", occurrence.RecurrenceRule);
    }

    [Fact]
    public async Task WeeklyRecurrence_DoesNotGenerateOnNonMatchingDay()
    {
        var fixture = CreateFixture();
        await fixture.Service.CreateScheduledAsync(
            NewScheduled(
                "Semanal",
                1,
                new DateOnly(2026, 10, 6),
                recurrence: RecurrenceKind.Weekly));

        var day12 = await fixture.Service.GetForDateAsync(new DateOnly(2026, 10, 12));
        Assert.Empty(day12);

        var day13 = await fixture.Service.GetForDateAsync(new DateOnly(2026, 10, 13));
        Assert.Single(day13);
    }

    [Fact]
    public async Task RecurrenceGeneration_IsIdempotent()
    {
        var fixture = CreateFixture();
        await fixture.Service.CreateScheduledAsync(
            NewScheduled(
                "Diária",
                1,
                new DateOnly(2026, 10, 6),
                recurrence: RecurrenceKind.Daily));

        await fixture.Service.GetForDateAsync(new DateOnly(2026, 10, 8));
        await fixture.Service.GetForDateAsync(new DateOnly(2026, 10, 8));

        Assert.Equal(
            3,
            fixture.Repository.Items.Count(x => x.RecurrenceRule == "FREQ=DAILY"));
    }

    [Fact]
    public async Task RecurringRoot_CannotBeHardDeleted()
    {
        var fixture = CreateFixture();
        var root = await fixture.Service.CreateScheduledAsync(
            NewScheduled(
                "Diária",
                1,
                new DateOnly(2026, 10, 6),
                recurrence: RecurrenceKind.Daily));

        await Assert.ThrowsAsync<DomainValidationException>(
            () => fixture.Service.DeleteAsync(root.Id));
    }

    private static CreateScheduledTaskRequest NewScheduled(
        string title,
        long categoryId,
        DateOnly date,
        TimeOnly? start = null,
        TimeOnly? end = null,
        RecurrenceKind recurrence = RecurrenceKind.None)
        => new(
            title,
            null,
            categoryId,
            TaskPriority.Normal,
            date,
            start,
            end,
            Recurrence: recurrence);

    private static Fixture CreateFixture()
    {
        var clock = new FakeTimeProvider(InitialNow);
        var categories = new[]
        {
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
            }
        };

        var tasks = new InMemoryTaskRepository();
        var categoryRepository = new InMemoryCategoryRepository(categories);
        var evaluator = new TaskTemporalEvaluator(clock);
        var temporal = new TemporalStateService(tasks, evaluator);
        var recurrence = new RecurrenceService(tasks, clock);
        var service = new TaskService(
            tasks,
            categoryRepository,
            evaluator,
            temporal,
            recurrence,
            clock);

        return new Fixture(service, tasks, clock, categories);
    }

    private sealed record Fixture(
        TaskService Service,
        InMemoryTaskRepository Repository,
        FakeTimeProvider Clock,
        IReadOnlyList<Category> Categories);

    private sealed class InMemoryCategoryRepository : ICategoryRepository
    {
        private readonly IReadOnlyList<Category> _categories;

        public InMemoryCategoryRepository(IReadOnlyList<Category> categories)
        {
            _categories = categories;
        }

        public Task<IReadOnlyList<Category>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(_categories);

        public Task<Category?> GetByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_categories.SingleOrDefault(x => x.Id == id));
    }

    private sealed class InMemoryTaskRepository : ITaskRepository
    {
        private readonly List<TaskItem> _tasks = [];
        private long _nextId = 1;

        public IReadOnlyList<TaskItem> Items => _tasks;

        public TaskItem Seed(TaskItem task)
        {
            if (task.Id == 0)
            {
                task.Id = _nextId++;
            }

            _tasks.Add(task);
            return task;
        }

        public Task<IReadOnlyList<TaskItem>> GetForDateAsync(
            DateOnly date,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TaskItem>>(
                _tasks.Where(x => x.CurrentPlannedDate == date).ToArray());

        public Task<TaskItem?> GetByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_tasks.SingleOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<TaskItem>> GetOpenAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TaskItem>>(
                _tasks.Where(x =>
                    x.Status == TaskState.Planned ||
                    x.Status == TaskState.InProgress).ToArray());

        public Task<IReadOnlyList<TaskItem>> GetRecurrenceRootsAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TaskItem>>(
                _tasks.Where(x =>
                    x.RecurrenceRule is not null &&
                    x.Origin == "RecurrenceRoot").ToArray());

        public Task<bool> RecurrenceOccurrenceExistsAsync(
            long recurrenceRootId,
            DateOnly occurrenceDate,
            CancellationToken cancellationToken = default)
        {
            var origin = $"Recurrence:{recurrenceRootId}";
            return Task.FromResult(
                _tasks.Any(x =>
                    x.Origin == origin &&
                    x.OriginalPlannedDate == occurrenceDate));
        }

        public Task<bool> RecoveryExistsAsync(
            long originalTaskId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                _tasks.Any(x => x.RecoveredFromTaskId == originalTaskId));

        public Task<TaskItem> AddAsync(
            TaskItem task,
            CancellationToken cancellationToken = default)
        {
            Seed(task);
            return Task.FromResult(task);
        }

        public Task AddManyAsync(
            IReadOnlyCollection<TaskItem> tasks,
            CancellationToken cancellationToken = default)
        {
            foreach (var task in tasks)
            {
                Seed(task);
            }

            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            TaskItem task,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateManyAsync(
            IReadOnlyCollection<TaskItem> tasks,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteAsync(
            TaskItem task,
            CancellationToken cancellationToken = default)
        {
            _tasks.Remove(task);
            return Task.CompletedTask;
        }
    }
}
