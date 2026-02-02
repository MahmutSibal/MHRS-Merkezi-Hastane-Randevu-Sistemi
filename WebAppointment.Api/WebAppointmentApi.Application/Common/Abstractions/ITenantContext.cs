namespace WebAppointmentApi.Application.Common.Abstractions;

public interface ITenantContext
{
    int TenantId { get; }
}
