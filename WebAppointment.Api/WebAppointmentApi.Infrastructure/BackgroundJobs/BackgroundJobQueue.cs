using System.Threading.Channels;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.Infrastructure.BackgroundJobs;

public sealed class BackgroundJobQueue : IBackgroundJobQueue
{
    private readonly Channel<BackgroundJob> _queue;

    public BackgroundJobQueue()
    {
        _queue = Channel.CreateUnbounded<BackgroundJob>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public void Enqueue(BackgroundJob job)
    {
        if (job is null)
        {
            throw new ArgumentNullException(nameof(job));
        }

        if (!_queue.Writer.TryWrite(job))
        {
            throw new InvalidOperationException("Background job queue is unavailable.");
        }
    }

    public async ValueTask<BackgroundJob> DequeueAsync(CancellationToken ct)
        => await _queue.Reader.ReadAsync(ct);
}
