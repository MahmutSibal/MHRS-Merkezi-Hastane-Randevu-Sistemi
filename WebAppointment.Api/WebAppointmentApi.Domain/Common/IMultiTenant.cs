namespace WebAppointmentApi.Domain.Common;

public interface IMultiTenant
{
    int TenantId { get; set; }
}
