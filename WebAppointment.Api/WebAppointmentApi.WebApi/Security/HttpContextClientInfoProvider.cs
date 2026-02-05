using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.WebApi.Security;

public sealed class HttpContextClientInfoProvider : IClientInfoProvider
{
    private readonly IHttpContextAccessor _http;

    public HttpContextClientInfoProvider(IHttpContextAccessor http)
    {
        _http = http;
    }

    public string? IpAddress => _http.HttpContext?.Connection?.RemoteIpAddress?.ToString();
}
