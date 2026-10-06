using DisciplineTimeline.Core.Exceptions;
using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Core.Repositories;
using DisciplineTimeline.Core.Requests;
using DisciplineTimeline.Core.Time;

namespace DisciplineTimeline.Core.Services;

public sealed class TaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly TaskTemporalEvaluator _temporalEvaluator;
    private readonly TimeProvider _timeProvider;

    public TaskService(
        ITaskRepository taskRepository,
        ICategoryRepository categoryRepository,
        TaskTemporalEvaluator temporalEvaluator,
        TimeProvider timeProvider)
    {
        _taskRepository = taskRepository;
        _categoryRepository = categoryRepository;
        _temporalEvaluator = temporalEvaluator;
        _timeProvider = timeProvider;
    }

    public Task<IReadOnlyList<TaskItem>> GetForDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
        => _taskRepository.GetForDateAsync(date, cancellationToken);

    public Task<IReadOnlyList<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
        => _categoryRepository.GetAllAsync(cancellationToken);

    public async Task<TaskItem> CreateScheduledAsync(
        CreateScheduledTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetLocalNow();
        var today = DateOnly.FromDateTime(now.DateTime);

        if (request.PlannedDate < today)
        {
            throw new DomainValidationException(
                "A criação agendada aceita hoje ou uma data futura. Datas passadas pertencem ao histórico.");
        }

        var category = await RequireCategoryAsync(request.CategoryId, cancellationToken);
        var title = ValidateTitle(request.Title);
        ValidateTimeWindow(request.PlannedStartTime, request.PlannedEndTime);

        var task = new TaskItem
        {
            Title = title,
            Description = NormalizeOptional(request.Description),
            CategoryId = category.Id,
            Category = category,
            Priority = request.Priority,
            CreatedAt = now,
            OriginalPlannedDate = request.PlannedDate,
            CurrentPlannedDate = request.PlannedDate,
            PlannedStartTime = request.PlannedStartTime,
            PlannedEndTime = request.PlannedEndTime,
            Status = TaskState.Planned,
            HasLateFlag = false,
            HasRescheduledFlag = false,
            RescheduleCount = 0,
            HasCancellationFlag = false,
            Origin = "Manual",
            CycleId = request.CycleId
        };

        return await _taskRepository.AddAsync(task, cancellationToken);
    }

    public async Task<TaskItem> StartNowAsync(
        StartNowTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetLocalNow();
        var today = DateOnly.FromDateTime(now.DateTime);
        var currentMinute = new TimeOnly(now.Hour, now.Minute);

        if (request.PlannedEndTime is { } endTime && endTime <= currentMinute)
        {
            throw new DomainValidationException(
                "O horário final de uma tarefa iniciada agora precisa ser posterior ao horário atual.");
        }

        var category = await RequireCategoryAsync(request.CategoryId, cancellationToken);
        var title = ValidateTitle(request.Title);

        var task = new TaskItem
        {
            Title = title,
            Description = NormalizeOptional(request.Description),
            CategoryId = category.Id,
            Category = category,
            Priority = request.Priority,
            CreatedAt = now,
            OriginalPlannedDate = today,
            CurrentPlannedDate = today,
            PlannedStartTime = currentMinute,
            PlannedEndTime = request.PlannedEndTime,
            ActualStartAt = now,
            Status = TaskState.InProgress,
            HasLateFlag = false,
            HasRescheduledFlag = false,
            RescheduleCount = 0,
            HasCancellationFlag = false,
            Origin = "Manual",
            CycleId = request.CycleId
        };

        return await _taskRepository.AddAsync(task, cancellationToken);
    }

    public async Task<TaskItem> UpdateAsync(
        long taskId,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);

        if (task.Status != TaskState.Planned ||
            task.ActualStartAt is not null ||
            task.CompletedAt is not null)
        {
            throw new DomainValidationException(
                "Somente tarefas ainda não iniciadas podem ser editadas nesta etapa. Alterações históricas terão fluxos próprios.");
        }

        var category = await RequireCategoryAsync(request.CategoryId, cancellationToken);
        var title = ValidateTitle(request.Title);
        ValidateTimeWindow(request.PlannedStartTime, request.PlannedEndTime);

        task.Title = title;
        task.Description = NormalizeOptional(request.Description);
        task.CategoryId = category.Id;
        task.Category = category;
        task.Priority = request.Priority;
        task.PlannedStartTime = request.PlannedStartTime;
        task.PlannedEndTime = request.PlannedEndTime;

        await _taskRepository.UpdateAsync(task, cancellationToken);
        return task;
    }

    public async Task<TaskItem> StartAsync(
        long taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);
        var now = _timeProvider.GetLocalNow();
        var today = DateOnly.FromDateTime(now.DateTime);

        if (task.Status != TaskState.Planned)
        {
            throw new DomainValidationException("Apenas tarefas planejadas podem ser iniciadas.");
        }

        if (task.CurrentPlannedDate != today)
        {
            throw new DomainValidationException(
                "Uma tarefa só pode ser iniciada pelo fluxo normal no dia operacional planejado.");
        }

        task.Status = TaskState.InProgress;
        task.ActualStartAt = now;

        await _taskRepository.UpdateAsync(task, cancellationToken);
        return task;
    }

    public async Task<TaskItem> CompleteAsync(
        long taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);

        if (task.Status is TaskState.Completed or TaskState.Cancelled or TaskState.Lost)
        {
            throw new DomainValidationException(
                "A tarefa não pode ser concluída a partir do estado atual.");
        }

        var evaluation = _temporalEvaluator.Evaluate(task);

        if (evaluation.IsLost)
        {
            throw new DomainValidationException(
                "A tarefa já pertence a um dia perdido. A conclusão posterior será tratada pelo fluxo de recuperação.");
        }

        task.HasLateFlag = task.HasLateFlag || evaluation.IsLate;
        task.CompletedAt = evaluation.EvaluatedAt;
        task.Status = TaskState.Completed;

        await _taskRepository.UpdateAsync(task, cancellationToken);
        return task;
    }

    public async Task DeleteAsync(
        long taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await RequireTaskAsync(taskId, cancellationToken);

        if (!CanHardDelete(task))
        {
            throw new DomainValidationException(
                "Só é possível excluir definitivamente uma tarefa planejada sem execução ou histórico. Demais casos devem permanecer auditáveis.");
        }

        await _taskRepository.DeleteAsync(task, cancellationToken);
    }

    private async Task<Category> RequireCategoryAsync(
        long categoryId,
        CancellationToken cancellationToken)
    {
        if (categoryId <= 0)
        {
            throw new DomainValidationException("Selecione uma categoria.");
        }

        return await _categoryRepository.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new DomainValidationException("A categoria selecionada não existe.");
    }

    private async Task<TaskItem> RequireTaskAsync(
        long taskId,
        CancellationToken cancellationToken)
    {
        if (taskId <= 0)
        {
            throw new DomainValidationException("Tarefa inválida.");
        }

        return await _taskRepository.GetByIdAsync(taskId, cancellationToken)
            ?? throw new DomainValidationException("A tarefa não foi encontrada.");
    }

    private static string ValidateTitle(string? title)
    {
        var normalized = title?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainValidationException("Informe o título da tarefa.");
        }

        if (normalized.Length > 240)
        {
            throw new DomainValidationException("O título deve ter no máximo 240 caracteres.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static void ValidateTimeWindow(TimeOnly? startTime, TimeOnly? endTime)
    {
        if (endTime is not null && startTime is null)
        {
            throw new DomainValidationException(
                "Para definir um horário final também é necessário informar o horário de início.");
        }

        if (startTime is { } start && endTime is { } end && end <= start)
        {
            throw new DomainValidationException(
                "O horário final precisa ser posterior ao horário de início.");
        }
    }

    private static bool CanHardDelete(TaskItem task)
        => task.Status == TaskState.Planned
           && task.ActualStartAt is null
           && task.CompletedAt is null
           && !task.HasLateFlag
           && !task.HasRescheduledFlag
           && task.RescheduleCount == 0
           && !task.HasCancellationFlag
           && task.RecoveredFromTaskId is null;
}
