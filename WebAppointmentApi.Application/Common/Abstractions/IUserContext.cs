namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IUserContext
{
    Guid UserId { get; }
    string Role { get; }
    string Email { get; }
}
