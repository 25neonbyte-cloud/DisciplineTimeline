using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DisciplineTimeline.Core.Exceptions;
using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Core.Recurrence;
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
    public partial int LostCount { get; set; }

    [ObservableProperty]
    public partial int BonusCount { get; set; }

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
    public partial RecurrenceKind SelectedRecurrence { get; set; } = RecurrenceKind.None;

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
    public IReadOnlyList<RecurrenceKind> RecurrenceOptions { get; } = Enum.GetValues<RecurrenceKind>();

    public bool IsCreateMode => SelectedTask is null;

    public MainViewModel(
        TaskService taskService,
        MetricsService metricsService,
        TaskTemporalEvaluator temporalEvaluator,
        TimeProvider timeProvider)
    {
        _taskService = taskService;
        _metricsService = metricsService;
        _temporalEvaluator = temporalEvaluator;
        _timeProvider = timeProvider;

        var today = _timeProvider.GetLocalNow().Date;
        SelectedDate = today;
        TaskPlannedDate = today;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await LoadCategoriesAsync(cancellationToken);
        await LoadCyclesAsync(cancellationToken);
        await LoadSelectedDateAsync(cancellationToken);
        _initializedMetrics = true;
        ClearEditor();
    }

    [RelayCommand]
    private async Task LoadSelectedDateAsync()
        => await LoadSelectedDateAsync(CancellationToken.None);

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await ExecuteUiActionAsync(async () =>
        {
            await _taskService.RunMaintenanceAsync();
            await LoadSelectedDateAsync(CancellationToken.None);
            StatusMessage = "Estados temporais atualizados.";
        });
    }

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
                ParseOptionalTime(PlannedEndText, "fim"),
                CycleId: CycleForDate(TaskPlannedDate),
                Recurrence: SelectedRecurrence);

            var created = await _taskService.CreateScheduledAsync(request);
            StatusMessage = SelectedRecurrence == RecurrenceKind.None
                ? $"Tarefa criada para {created.CurrentPlannedDate:dd/MM/yyyy}."
                : $"Tarefa recorrente criada a partir de {created.CurrentPlannedDate:dd/MM/yyyy}.";

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
                ParseOptionalTime(PlannedEndText, "fim"),
                CycleId: CycleForDate(_timeProvider.GetLocalNow().Date));

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
    private async Task RescheduleSelectedAsync()
    {
        if (SelectedTask is null)
        {
            StatusMessage = "Selecione uma tarefa para reagendar.";
            return;
        }

        await ExecuteUiActionAsync(async () =>
        {
            var rescheduled = await _taskService.RescheduleAsync(
                SelectedTask.Id,
                DateOnly.FromDateTime(TaskPlannedDate));

            StatusMessage =
                $"Reagendada para {rescheduled.CurrentPlannedDate:dd/MM/yyyy}; data original preservada em {rescheduled.OriginalPlannedDate:dd/MM/yyyy}.";

            SelectedDate = rescheduled.CurrentPlannedDate.ToDateTime(TimeOnly.MinValue);
            await LoadSelectedDateAsync(CancellationToken.None);
            SelectById(rescheduled.Id);
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
            StatusMessage = started.HasLateFlag
                ? "Tarefa iniciada com atraso histórico registrado."
                : "Tarefa iniciada.";

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
    private async Task CancelSelectedAsync()
    {
        if (SelectedTask is null)
        {
            StatusMessage = "Selecione uma tarefa para cancelar.";
            return;
        }

        await ExecuteUiActionAsync(async () =>
        {
            var cancelled = await _taskService.CancelAsync(SelectedTask.Id);
            StatusMessage = cancelled.HasLateFlag
                ? "Tarefa cancelada; atraso anterior foi preservado."
                : "Tarefa cancelada.";

            await LoadSelectedDateAsync(CancellationToken.None);
            SelectById(cancelled.Id);
        });
    }

    [RelayCommand]
    private async Task RecoverSelectedAsync()
    {
        if (SelectedTask is null)
        {
            StatusMessage = "Selecione uma tarefa perdida para recuperar.";
            return;
        }

        await ExecuteUiActionAsync(async () =>
        {
            var recovered = await _taskService.RecoverAsync(SelectedTask.Id);
            StatusMessage = "Bônus recuperado registrado sem alterar o dia perdido.";

            SelectedDate = recovered.CurrentPlannedDate.ToDateTime(TimeOnly.MinValue);
            await LoadSelectedDateAsync(CancellationToken.None);
            SelectById(recovered.Id);
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
        SelectedRecurrence = RecurrenceRuleFactory.Parse(value.RecurrenceRule);
        TaskPlannedDate = value.CurrentPlannedDate.ToDateTime(TimeOnly.MinValue);
        PlannedStartText = value.PlannedStartTime?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;
        PlannedEndText = value.PlannedEndTime?.ToString("HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;
        StatusMessage =
            $"Tarefa #{value.Id}. Original: {value.OriginalPlannedDate:dd/MM/yyyy}; atual: {value.CurrentPlannedDate:dd/MM/yyyy}.";
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
        => await LoadTimelineMetricsAsync(cancellationToken);

    private void ClearEditor(bool keepStatusMessage = false)
    {
        SelectedTask = null;
        TaskTitle = string.Empty;
        TaskDescription = string.Empty;
        SelectedPriority = TaskPriority.Normal;
        SelectedRecurrence = RecurrenceKind.None;
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
