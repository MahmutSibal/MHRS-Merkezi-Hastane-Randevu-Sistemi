namespace WebAppointmentApi.Application.Common.Abstractions;

public interface IClientInfoProvider
{
    string? IpAddress { get; }
}
