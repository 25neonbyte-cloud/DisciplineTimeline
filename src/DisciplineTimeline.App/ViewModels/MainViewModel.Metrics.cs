using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DisciplineTimeline.Core.Metrics;
using DisciplineTimeline.Core.Models;
using DisciplineTimeline.Core.Services;

namespace DisciplineTimeline.App.ViewModels;

public partial class MainViewModel
{
    private readonly MetricsService _metricsService;
    private bool _loadingCycles;
    private bool _initializedMetrics;

    [ObservableProperty]
    public partial string ExecutionLabel { get; set; } = "—";

    [ObservableProperty]
    public partial int MovedAwayCount { get; set; }

    [ObservableProperty]
    public partial PeriodMetrics? MonthlyMetrics { get; set; }

    [ObservableProperty]
    public partial PeriodMetrics? CycleMetrics { get; set; }

    [ObservableProperty]
    public partial string MonthLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial PlanningCycle? SelectedCycle { get; set; }

    public string MonthlyDeliveryLabel => FormatPercentage(MonthlyMetrics?.DeliveryPercent);
    public string MonthlyConsistencyLabel => FormatPercentage(MonthlyMetrics?.ConsistencyPercent);
    public string CycleDeliveryLabel => FormatPercentage(CycleMetrics?.DeliveryPercent);
    public string CycleConsistencyLabel => FormatPercentage(CycleMetrics?.ConsistencyPercent);

    public ObservableCollection<TimelineDayNode> TimelineDays { get; } = [];
    public ObservableCollection<PlanningCycle> Cycles { get; } = [];

    [RelayCommand]
    private async Task PreviousDayAsync()
        => await NavigateToDayAsync(SelectedDate.AddDays(-1));

    [RelayCommand]
    private async Task NextDayAsync()
        => await NavigateToDayAsync(SelectedDate.AddDays(1));

    [RelayCommand]
    private async Task GoTodayAsync()
        => await NavigateToDayAsync(_timeProvider.GetLocalNow().Date);

    [RelayCommand]
    private async Task NavigateToDayAsync(DateTime date)
    {
        await ExecuteUiActionAsync(async () =>
        {
            SelectedDate = date.Date;
            await LoadSelectedDateAsync(CancellationToken.None);
        });
    }

    partial void OnSelectedCycleChanged(PlanningCycle? value)
    {
        if (_initializedMetrics && !_loadingCycles)
        {
            _ = ExecuteUiActionAsync(() => LoadSelectedDateAsync(CancellationToken.None));
        }
    }

    partial void OnMonthlyMetricsChanged(PeriodMetrics? value)
    {
        OnPropertyChanged(nameof(MonthlyDeliveryLabel));
        OnPropertyChanged(nameof(MonthlyConsistencyLabel));
    }

    partial void OnCycleMetricsChanged(PeriodMetrics? value)
    {
        OnPropertyChanged(nameof(CycleDeliveryLabel));
        OnPropertyChanged(nameof(CycleConsistencyLabel));
    }

    private async Task LoadCyclesAsync(CancellationToken cancellationToken)
    {
        _loadingCycles = true;
        try
        {
            Cycles.Clear();
            foreach (var cycle in await _metricsService.GetCyclesAsync(cancellationToken))
            {
                Cycles.Add(cycle);
            }

            var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
            SelectedCycle = Cycles.FirstOrDefault(c =>
                c.StartDate <= today && c.EndDate >= today) ?? Cycles.FirstOrDefault();
        }
        finally
        {
            _loadingCycles = false;
        }
    }

    private long? CycleForDate(DateTime date)
    {
        var planned = DateOnly.FromDateTime(date);
        return Cycles.FirstOrDefault(c =>
            c.StartDate <= planned && c.EndDate >= planned)?.Id;
    }

    private async Task LoadTimelineMetricsAsync(CancellationToken cancellationToken)
    {
        var selected = DateOnly.FromDateTime(SelectedDate);
        var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
        var culture = CultureInfo.GetCultureInfo("pt-BR");

        SelectedDateLabel = SelectedDate.ToString(
            "dddd, dd 'de' MMMM 'de' yyyy", culture);

        var timelineStart = selected.AddDays(-5);
        var timelineEnd = selected.AddDays(5);
        var monthStart = new DateOnly(selected.Year, selected.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        MonthLabel = SelectedDate.ToString("MMMM 'de' yyyy", culture);

        var rangeStart = timelineStart < monthStart ? timelineStart : monthStart;
        var rangeEnd = timelineEnd > monthEnd ? timelineEnd : monthEnd;

        if (SelectedCycle is not null)
        {
            if (SelectedCycle.StartDate < rangeStart)
            {
                rangeStart = SelectedCycle.StartDate;
            }
            if (SelectedCycle.EndDate > rangeEnd)
            {
                rangeEnd = SelectedCycle.EndDate;
            }
        }

        // As recorrências são materializadas até o fim da janela de análise.
        await _taskService.GetForDateAsync(rangeEnd, cancellationToken);
        var items = await _taskService.GetForDateAsync(selected, cancellationToken);
        var snapshot = await _metricsService.GetSnapshotAsync(
            rangeStart, rangeEnd, cancellationToken);

        Tasks.Clear();
        foreach (var item in items)
        {
            Tasks.Add(item);
        }

        var daily = snapshot.ForDay(selected);
        PlannedCount = daily.Planned;
        CompletedCount = daily.Completed;
        LostCount = daily.Lost;
        LateCount = daily.Late;
        BonusCount = daily.RecoveredBonuses;
        MovedAwayCount = daily.MovedAwayFromOriginalDate;
        ExecutionLabel = FormatPercentage(daily.ExecutionPercent);
        ConsistencyLabel = FormatPercentage(daily.ConsistencyPercent);

        MonthlyMetrics = snapshot.ForPeriod(monthStart, monthEnd);
        CycleMetrics = SelectedCycle is null
            ? null
            : snapshot.ForPeriod(SelectedCycle.StartDate, SelectedCycle.EndDate);

        TimelineDays.Clear();
        for (var day = timelineStart; day <= timelineEnd; day = day.AddDays(1))
        {
            TimelineDays.Add(new TimelineDayNode(snapshot.ForDay(day), selected, today));
        }
    }

    private static string FormatPercentage(double? percentage)
        => percentage.HasValue
            ? $"{percentage.Value.ToString("0.#", CultureInfo.GetCultureInfo("pt-BR"))}%"
            : "—";
}
