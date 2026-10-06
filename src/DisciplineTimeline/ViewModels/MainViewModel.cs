using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DisciplineTimeline.Models;
using DisciplineTimeline.Services;

namespace DisciplineTimeline.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _database;

    public MainViewModel(DatabaseService database)
    {
        _database = database;
    }

    public ObservableCollection<TaskItem> Tasks { get; } = [];

    public string SelectedDateLabel => DateTime.Today.ToString("dddd, dd 'de' MMMM 'de' yyyy");

    public int PlannedCount => Tasks.Count;

    public int CompletedCount => Tasks.Count(t => t.Status == TaskStatus.Completed);

    public string ConsistencyLabel
    {
        get
        {
            if (PlannedCount == 0)
            {
                return "—";
            }

            var completedOnPlannedDate = Tasks.Count(t =>
                t.Status == TaskStatus.Completed &&
                t.CompletedAt is not null &&
                DateOnly.FromDateTime(t.CompletedAt.Value) == t.OriginalPlannedDate);

            return $"{completedOnPlannedDate * 100d / PlannedCount:0}%";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public async Task LoadAsync()
    {
        Tasks.Clear();

        var items = await _database.GetTasksForDateAsync(DateOnly.FromDateTime(DateTime.Today));

        foreach (var item in items)
        {
            Tasks.Add(item);
        }

        OnPropertyChanged(nameof(PlannedCount));
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(ConsistencyLabel));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
