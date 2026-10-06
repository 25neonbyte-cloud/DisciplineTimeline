using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DisciplineTimeline.Core.Exceptions;
using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Core.Requests;
using DisciplineTimeline.Core.Services;
using DisciplineTimeline.Core.Time;

namespace DisciplineTimeline.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly TaskService _taskService;
    private readonly TaskTemporalEvaluator _temporalEvaluator;
    private readonly TimeProvider _timeProvider;

    [ObservableProperty]
    public partial string SelectedDateLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime SelectedDate { get; set; }

    [ObservableProperty]
    public partial int PlannedCount { get; set; }

    [ObservableProperty]
    public partial int CompletedCount { get; set; }

    [ObservableProperty]
    public partial int LateCount { get; set; }

    [ObservableProperty]
    public partial string ConsistencyLabel { get; set; } = "—";

    [ObservableProperty]
    public partial TaskItem? SelectedTask { get; set; }

    [ObservableProperty]
    public partial string TaskTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TaskDescription { get; set; } = string.Empty;

    [ObservableProperty]
    public partial long SelectedCategoryId { get; set; }

    [ObservableProperty]
    public partial TaskPriority SelectedPriority { get; set; } = TaskPriority.Normal;

    [ObservableProperty]
    public partial DateTime TaskPlannedDate { get; set; }

    [ObservableProperty]
    public partial string PlannedStartText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlannedEndText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    public ObservableCollection<TaskItem> Tasks { get; } = [];
    public ObservableCollection<Category> Categories { get; } = [];
    public IReadOnlyList<TaskPriority> PriorityOptions { get; } = Enum.GetValues<TaskPriority>();

    public bool IsCreateMode => SelectedTask is null;

    public MainViewModel(
        TaskService taskService,
        TaskTemporalEvaluator temporalEvaluator,
        TimeProvider timeProvider)
    {
        _taskService = taskService;
        _temporalEvaluator = temporalEvaluator;
        _timeProvider = timeProvider;

        var today = _timeProvider.GetLocalNow().Date;
        SelectedDate = today;
        TaskPlannedDate = today;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await LoadCategoriesAsync(cancellationToken);
        await LoadSelectedDateAsync(cancellationToken);
        ClearEditor();
    }

    [RelayCommand]
    private async Task LoadSelectedDateAsync()
        => await LoadSelectedDateAsync(CancellationToken.None);

    [RelayCommand]
    private void NewTask()
    {
        ClearEditor();
        StatusMessage = "Nova tarefa.";
    }

    [RelayCommand]
    private async Task CreateScheduledAsync()
    {
        await ExecuteUiActionAsync(async () =>
        {
            var request = new CreateScheduledTaskRequest(
                TaskTitle,
                TaskDescription,
                SelectedCategoryId,
                SelectedPriority,
                DateOnly.FromDateTime(TaskPlannedDate),
                ParseOptionalTime(PlannedStartText, "início"),
                ParseOptionalTime(PlannedEndText, "fim"));

            var created = await _taskService.CreateScheduledAsync(request);
            StatusMessage = $"Tarefa criada para {created.CurrentPlannedDate:dd/MM/yyyy}.";

            SelectedDate = created.CurrentPlannedDate.ToDateTime(TimeOnly.MinValue);
            await LoadSelectedDateAsync(CancellationToken.None);
            ClearEditor(keepStatusMessage: true);
        });
    }

    [RelayCommand]
    private async Task StartNowAsync()
    {
        await ExecuteUiActionAsync(async () =>
        {
            var request = new StartNowTaskRequest(
                TaskTitle,
                TaskDescription,
                SelectedCategoryId,
                SelectedPriority,
                ParseOptionalTime(PlannedEndText, "fim"));

            var created = await _taskService.StartNowAsync(request);
            StatusMessage = $"Tarefa iniciada às {created.ActualStartAt:HH:mm}.";

            SelectedDate = _timeProvider.GetLocalNow().Date;
            await LoadSelectedDateAsync(CancellationToken.None);
            SelectById(created.Id);
        });
    }

    [RelayCommand]
    private async Task SaveEditAsync()
    {
        if (SelectedTask is null)
        {
            StatusMessage = "Selecione uma tarefa para editar.";
            return;
        }

        await ExecuteUiActionAsync(async () =>
        {
            var request = new UpdateTaskRequest(
                TaskTitle,
                TaskDescription,
                SelectedCategoryId,
                SelectedPriority,
                ParseOptionalTime(PlannedStartText, "início"),
                ParseOptionalTime(PlannedEndText, "fim"));

            var updated = await _taskService.UpdateAsync(SelectedTask.Id, request);
            StatusMessage = "Tarefa atualizada.";

            await LoadSelectedDateAsync(CancellationToken.None);
            SelectById(updated.Id);
        });
    }

    [RelayCommand]
    private async Task StartSelectedAsync()
    {
        if (SelectedTask is null)
        {
            StatusMessage = "Selecione uma tarefa.";
            return;
        }

        await ExecuteUiActionAsync(async () =>
        {
            var started = await _taskService.StartAsync(SelectedTask.Id);
            StatusMessage = "Tarefa iniciada.";

            await LoadSelectedDateAsync(CancellationToken.None);
            SelectById(started.Id);
        });
    }

    [RelayCommand]
    private async Task CompleteSelectedAsync()
    {
        if (SelectedTask is null)
        {
            StatusMessage = "Selecione uma tarefa.";
            return;
        }

        await ExecuteUiActionAsync(async () =>
        {
            var completed = await _taskService.CompleteAsync(SelectedTask.Id);
            StatusMessage = completed.HasLateFlag
                ? "Tarefa concluída com flag histórica de atraso."
                : "Tarefa concluída.";

            await LoadSelectedDateAsync(CancellationToken.None);
            SelectById(completed.Id);
        });
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedTask is null)
        {
            StatusMessage = "Selecione uma tarefa.";
            return;
        }

        await ExecuteUiActionAsync(async () =>
        {
            await _taskService.DeleteAsync(SelectedTask.Id);
            StatusMessage = "Tarefa planejada excluída.";

            await LoadSelectedDateAsync(CancellationToken.None);
            ClearEditor(keepStatusMessage: true);
        });
    }

    partial void OnSelectedTaskChanged(TaskItem? value)
    {
        OnPropertyChanged(nameof(IsCreateMode));

        if (value is null)
        {
            return;
        }

        TaskTitle = value.Title;
        TaskDescription = value.Description ?? string.Empty;
        SelectedCategoryId = value.CategoryId;
        SelectedPriority = value.Priority;
        TaskPlannedDate = value.CurrentPlannedDate.ToDateTime(TimeOnly.MinValue);
        PlannedStartText = value.PlannedStartTime?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;
        PlannedEndText = value.PlannedEndTime?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;
        StatusMessage = $"Editando tarefa #{value.Id}. A data não é alterada por este formulário.";
    }

    private async Task LoadCategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = await _taskService.GetCategoriesAsync(cancellationToken);

        Categories.Clear();
        foreach (var category in categories)
        {
            Categories.Add(category);
        }

        if (SelectedCategoryId == 0 && Categories.Count > 0)
        {
            SelectedCategoryId = Categories[0].Id;
        }
    }

    private async Task LoadSelectedDateAsync(CancellationToken cancellationToken)
    {
        var date = DateOnly.FromDateTime(SelectedDate);
        SelectedDateLabel = SelectedDate.ToString("dddd, dd 'de' MMMM 'de' yyyy");

        var items = await _taskService.GetForDateAsync(date, cancellationToken);

        Tasks.Clear();
        foreach (var item in items)
        {
            Tasks.Add(item);
        }

        var plannedItems = items
            .Where(x => x.RecoveredFromTaskId is null)
            .ToArray();

        PlannedCount = plannedItems.Length;
        CompletedCount = plannedItems.Count(x => x.Status == TaskState.Completed);
        LateCount = plannedItems.Count(x => _temporalEvaluator.Evaluate(x).IsLate);

        if (PlannedCount == 0)
        {
            ConsistencyLabel = "—";
            return;
        }

        var completedOnOperationalDate = plannedItems.Count(x =>
            x.Status == TaskState.Completed &&
            x.CompletedAt is not null &&
            DateOnly.FromDateTime(x.CompletedAt.Value.LocalDateTime) == x.CurrentPlannedDate);

        ConsistencyLabel = $"{completedOnOperationalDate * 100d / PlannedCount:0}%";
    }

    private void ClearEditor(bool keepStatusMessage = false)
    {
        SelectedTask = null;
        TaskTitle = string.Empty;
        TaskDescription = string.Empty;
        SelectedPriority = TaskPriority.Normal;
        TaskPlannedDate = _timeProvider.GetLocalNow().Date;
        PlannedStartText = string.Empty;
        PlannedEndText = string.Empty;

        if (Categories.Count > 0)
        {
            SelectedCategoryId = Categories[0].Id;
        }

        if (!keepStatusMessage)
        {
            StatusMessage = string.Empty;
        }
    }

    private void SelectById(long taskId)
    {
        SelectedTask = Tasks.FirstOrDefault(x => x.Id == taskId);
    }

    private async Task ExecuteUiActionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (DomainValidationException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Falha inesperada: {ex.Message}";
        }
    }

    private static TimeOnly? ParseOptionalTime(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (TimeOnly.TryParseExact(
                value.Trim(),
                "HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            return parsed;
        }

        throw new DomainValidationException(
            $"Horário de {fieldName} inválido. Use o formato HH:mm.");
    }
}
