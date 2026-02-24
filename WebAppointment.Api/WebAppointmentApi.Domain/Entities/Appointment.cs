using WebAppointmentApi.Domain.Common;
using WebAppointmentApi.Domain.Enums;

namespace WebAppointmentApi.Domain.Entities;

public sealed class Appointment : IMultiTenant
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public int? DependentId { get; set; }
    public Dependent? Dependent { get; set; }

    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }

    public AppointmentStatus Status { get; set; }

    public DateTimeOffset? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }

    public DateTimeOffset? ReminderSentAtUtc { get; set; }

    public int TenantId { get; set; }
}
