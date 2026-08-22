namespace WebAppointmentApi.Application.Common.Exceptions;

public sealed class EmailVerificationRequiredException : Exception
{
    public EmailVerificationRequiredException(string message) : base(message) { }
}
