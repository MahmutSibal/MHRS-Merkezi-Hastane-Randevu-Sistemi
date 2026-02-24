using WebAppointmentApi.Domain.Common;

namespace WebAppointmentApi.Domain.Entities;

public sealed class Patient : IMultiTenant
{
    public int Id { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string TcKimlikNo { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }

    public bool IsDeleted { get; set; }

    public int TenantId { get; set; }

    // No-show risk score (0-100)
    public int NoShowScore { get; set; }

    // Health profile (patient fills from profile page)
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicDiseases { get; set; }
    public string? Medications { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}
