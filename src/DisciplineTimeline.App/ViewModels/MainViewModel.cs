using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Core.Repositories;
using DisciplineTimeline.Core.Time;

namespace DisciplineTimeline.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ITaskRepository _taskRepository;
    private readonly TaskTemporalEvaluator _temporalEvaluator;
    private readonly TimeProvider _timeProvider;

    [ObservableProperty]
    private string selectedDateLabel = string.Empty;

    [ObservableProperty]
    private int plannedCount;

    [ObservableProperty]
    private int completedCount;

    [ObservableProperty]
    private int lateCount;

    [ObservableProperty]
    private string consistencyLabel = "—";

    public ObservableCollection<TaskItem> Tasks { get; } = [];

    public MainViewModel(
        ITaskRepository taskRepository,
        TaskTemporalEvaluator temporalEvaluator,
        TimeProvider timeProvider)
    {
        _taskRepository = taskRepository;
        _temporalEvaluator = temporalEvaluator;
        _timeProvider = timeProvider;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetLocalNow();
        var today = DateOnly.FromDateTime(now.DateTime);

        SelectedDateLabel = now.ToString("dddd, dd 'de' MMMM 'de' yyyy");

        var items = await _taskRepository.GetForDateAsync(today, cancellationToken);

        Tasks.Clear();
        foreach (var item in items)
        {
            Tasks.Add(item);
        }

        var plannedItems = items
            .Where(x => x.RecoveredFromTaskId is null)
            .ToArray();

        PlannedCount = plannedItems.Length;
        CompletedCount = plannedItems.Count(x => x.Status == TaskStatus.Completed);
        LateCount = plannedItems.Count(x => _temporalEvaluator.Evaluate(x).IsLate);

        if (PlannedCount == 0)
        {
            ConsistencyLabel = "—";
            return;
        }

        var completedOnOperationalDate = plannedItems.Count(x =>
            x.Status == TaskStatus.Completed &&
            x.CompletedAt is not null &&
            DateOnly.FromDateTime(x.CompletedAt.Value.LocalDateTime) == x.CurrentPlannedDate);

        ConsistencyLabel = $"{completedOnOperationalDate * 100d / PlannedCount:0}%";
    }
}
