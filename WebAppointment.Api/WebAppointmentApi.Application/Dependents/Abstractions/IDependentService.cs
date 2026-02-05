using WebAppointmentApi.Application.Dependents.Dtos;

namespace WebAppointmentApi.Application.Dependents.Abstractions;

public interface IDependentService
{
    Task<IReadOnlyList<DependentDto>> ListMyAsync(Guid userId, CancellationToken ct);
    Task<DependentDto> CreateAsync(Guid userId, CreateDependentRequest request, CancellationToken ct);
    Task DeleteAsync(Guid userId, int dependentId, CancellationToken ct);
}
