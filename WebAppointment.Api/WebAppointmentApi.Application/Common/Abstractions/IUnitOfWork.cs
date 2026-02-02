using System.Data;

namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IUnitOfWork
{
    Task BeginAsync(CancellationToken ct, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync(CancellationToken ct);
}
