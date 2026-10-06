using System.Windows;
using DisciplineTimeline.Services;
using DisciplineTimeline.ViewModels;

namespace DisciplineTimeline;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(DatabaseService database)
    {
        InitializeComponent();
        _viewModel = new MainViewModel(database);
        DataContext = _viewModel;

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }
}
