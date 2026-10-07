using System.Windows;
using DisciplineTimeline.App.Services;
using DisciplineTimeline.App.ViewModels;
using DisciplineTimeline.Core.Services;
using DisciplineTimeline.Core.Time;
using DisciplineTimeline.Infrastructure;
using DisciplineTimeline.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DisciplineTimeline.App;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Logging.ClearProviders();
            builder.Logging.AddDebug();

            builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
            builder.Services.AddSingleton<TaskTemporalEvaluator>();
            builder.Services.AddSingleton<TemporalStateService>();
            builder.Services.AddSingleton<RecurrenceService>();
            builder.Services.AddSingleton<TaskService>();
            builder.Services.AddDisciplineTimelineInfrastructure();
            builder.Services.AddHostedService<TemporalMaintenanceHostedService>();

            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainWindow>();

            _host = builder.Build();
            await _host.StartAsync();

            var initializer = _host.Services.GetRequiredService<DatabaseInitializer>();
            await initializer.InitializeAsync();

            var window = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Falha ao inicializar o Discipline Timeline.\n\n{ex.Message}",
                "Discipline Timeline",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
