using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

public sealed class DoctorAvailability : IMultiTenant
{
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public TimeOnly WorkStart { get; set; }
    public TimeOnly WorkEnd { get; set; }

    public TimeOnly? LunchStart { get; set; }
    public TimeOnly? LunchEnd { get; set; }

    public int SlotMinutes { get; set; } = 30;

    public int TenantId { get; set; }
}
