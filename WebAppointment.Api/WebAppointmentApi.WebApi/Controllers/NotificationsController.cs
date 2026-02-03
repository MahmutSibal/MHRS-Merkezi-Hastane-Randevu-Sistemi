using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Notifications.Abstractions;
using WebAppointmentApi.Application.Notifications.Dtos;
using WebAppointmentApi.Application.Common.Abstractions;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;
    private readonly IUserContext _user;

    public NotificationsController(INotificationService notifications, IUserContext user)
    {
        _notifications = notifications;
        _user = user;
    }

    [HttpGet]
    public Task<IReadOnlyList<NotificationDto>> Get([FromQuery] int take = 10, CancellationToken ct = default)
        => _notifications.GetMyAsync(_user.UserId, take, ct);
}
