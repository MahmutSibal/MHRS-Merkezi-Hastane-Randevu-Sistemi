using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.Infrastructure.BackgroundJobs;

public sealed class QueuedBackgroundService : BackgroundService
{
    private readonly IBackgroundJobQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QueuedBackgroundService> _logger;

    public QueuedBackgroundService(
        IBackgroundJobQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<QueuedBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background job worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _queue.DequeueAsync(stoppingToken);
                using var scope = _scopeFactory.CreateScope();
                await job(scope.ServiceProvider, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background job failed.");
            }
        }
    }
}
