using DisciplineTimeline.Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DisciplineTimeline.App.Services;

public sealed class TemporalMaintenanceHostedService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    private readonly TaskService _taskService;
    private readonly ILogger<TemporalMaintenanceHostedService> _logger;

    public TemporalMaintenanceHostedService(
        TaskService taskService,
        ILogger<TemporalMaintenanceHostedService> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunMaintenanceSafelyAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunMaintenanceSafelyAsync(stoppingToken);
        }
    }

    private async Task RunMaintenanceSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _taskService.RunMaintenanceAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao sincronizar estados temporais.");
        }
    }
}
