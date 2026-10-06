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
    private static readonly DateTimeOffset Now =
        new(2026, 10, 6, 14, 30, 15, TimeSpan.Zero);

    [Fact]
    public async Task CreateScheduled_PreservesOriginalAndCurrentDate()
    {
        var fixture = CreateFixture();
        var plannedDate = new DateOnly(2026, 10, 8);

        var created = await fixture.Service.CreateScheduledAsync(
            new CreateScheduledTaskRequest(
                "  Auditoria  ",
                null,
                1,
                TaskPriority.High,
                plannedDate,
                new TimeOnly(9, 0),
                new TimeOnly(10, 0)));

        Assert.Equal("Auditoria", created.Title);
        Assert.Equal(plannedDate, created.OriginalPlannedDate);
        Assert.Equal(plannedDate, created.CurrentPlannedDate);
        Assert.Equal(TaskState.Planned, created.Status);
        Assert.False(created.HasLateFlag);
        Assert.False(created.HasRescheduledFlag);
    }

    [Fact]
    public async Task StartNow_CreatesInProgressTaskOnCurrentDate()
    {
        var fixture = CreateFixture();

        var created = await fixture.Service.StartNowAsync(
            new StartNowTaskRequest(
                "Começar agora",
                null,
                1,
                TaskPriority.Normal,
                new TimeOnly(16, 0)));

        Assert.Equal(new DateOnly(2026, 10, 6), created.OriginalPlannedDate);
        Assert.Equal(created.OriginalPlannedDate, created.CurrentPlannedDate);
        Assert.Equal(new TimeOnly(14, 30), created.PlannedStartTime);
        Assert.Equal(Now, created.ActualStartAt);
        Assert.Equal(TaskState.InProgress, created.Status);
    }

    [Fact]
    public async Task Update_DoesNotChangePlanningDates()
    {
        var fixture = CreateFixture();
        var created = await fixture.Service.CreateScheduledAsync(
            new CreateScheduledTaskRequest(
                "Original",
                null,
                1,
                TaskPriority.Normal,
                new DateOnly(2026, 10, 8),
                null,
                null));

        var updated = await fixture.Service.UpdateAsync(
            created.Id,
            new UpdateTaskRequest(
                "Alterada",
                "Descrição",
                2,
                TaskPriority.High,
                new TimeOnly(10, 0),
                new TimeOnly(11, 0)));

        Assert.Equal("Alterada", updated.Title);
        Assert.Equal(new DateOnly(2026, 10, 8), updated.OriginalPlannedDate);
        Assert.Equal(new DateOnly(2026, 10, 8), updated.CurrentPlannedDate);
        Assert.False(updated.HasRescheduledFlag);
        Assert.Equal(0, updated.RescheduleCount);
    }

    [Fact]
    public async Task CompleteAfterDeadline_PersistsLateFlag()
    {
        var fixture = CreateFixture();
        var created = await fixture.Service.CreateScheduledAsync(
            new CreateScheduledTaskRequest(
                "Prazo",
                null,
                1,
                TaskPriority.Normal,
                new DateOnly(2026, 10, 6),
                new TimeOnly(13, 0),
                new TimeOnly(14, 0)));

        var completed = await fixture.Service.CompleteAsync(created.Id);

        Assert.Equal(TaskState.Completed, completed.Status);
        Assert.True(completed.HasLateFlag);
        Assert.Equal(Now, completed.CompletedAt);
    }

    [Fact]
    public async Task Delete_RejectsTaskWithExecutionHistory()
    {
        var fixture = CreateFixture();
        var created = await fixture.Service.StartNowAsync(
            new StartNowTaskRequest(
                "Em andamento",
                null,
                1,
                TaskPriority.Normal,
                null));

        await Assert.ThrowsAsync<DomainValidationException>(
            () => fixture.Service.DeleteAsync(created.Id));
    }

    [Fact]
    public async Task ScheduledCreation_RejectsPastDate()
    {
        var fixture = CreateFixture();

        await Assert.ThrowsAsync<DomainValidationException>(
            () => fixture.Service.CreateScheduledAsync(
                new CreateScheduledTaskRequest(
                    "Passado",
                    null,
                    1,
                    TaskPriority.Normal,
                    new DateOnly(2026, 10, 5),
                    null,
                    null)));
    }

    private static Fixture CreateFixture()
    {
        var clock = new FakeTimeProvider(Now);
        var tasks = new InMemoryTaskRepository();
        var categories = new InMemoryCategoryRepository();
        var evaluator = new TaskTemporalEvaluator(clock);

        return new Fixture(
            new TaskService(tasks, categories, evaluator, clock));
    }

    private sealed record Fixture(TaskService Service);

    private sealed class InMemoryCategoryRepository : ICategoryRepository
    {
        private readonly List<Category> _categories =
        [
            new()
            {
                Id = 1,
                Name = "Miscelânia",
                RecoveryPolicy = RecoveryPolicy.Recoverable,
                IsSystemCategory = true
            },
            new()
            {
                Id = 2,
                Name = "Exercícios",
                RecoveryPolicy = RecoveryPolicy.NonRecoverable,
                IsSystemCategory = true
            }
        ];

        public Task<IReadOnlyList<Category>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Category>>(_categories);

        public Task<Category?> GetByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_categories.SingleOrDefault(x => x.Id == id));
    }

    private sealed class InMemoryTaskRepository : ITaskRepository
    {
        private readonly List<TaskItem> _tasks = [];
        private long _nextId = 1;

        public Task<IReadOnlyList<TaskItem>> GetForDateAsync(
            DateOnly date,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<TaskItem>>(
                _tasks.Where(x => x.CurrentPlannedDate == date).ToArray());

        public Task<TaskItem?> GetByIdAsync(
            long id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_tasks.SingleOrDefault(x => x.Id == id));

        public Task<TaskItem> AddAsync(
            TaskItem task,
            CancellationToken cancellationToken = default)
        {
            task.Id = _nextId++;
            _tasks.Add(task);
            return Task.FromResult(task);
        }

        public Task UpdateAsync(
            TaskItem task,
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
