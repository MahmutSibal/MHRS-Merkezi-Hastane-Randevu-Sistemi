namespace WebAppointmentApi.Application.Common.Abstractions;

public delegate Task BackgroundJob(IServiceProvider serviceProvider, CancellationToken ct);

public interface IBackgroundJobQueue
{
    void Enqueue(BackgroundJob job);
    ValueTask<BackgroundJob> DequeueAsync(CancellationToken ct);
}
