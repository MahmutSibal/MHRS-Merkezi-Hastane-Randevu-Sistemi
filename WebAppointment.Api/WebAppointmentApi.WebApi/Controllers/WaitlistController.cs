using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Waitlist.Abstractions;
using WebAppointmentApi.Application.Waitlist.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/waitlist")]
[Authorize(Roles = "Patient")]
public sealed class WaitlistController : ControllerBase
{
    private readonly IUserContext _user;
    private readonly IWaitlistService _waitlist;

    public WaitlistController(IUserContext user, IWaitlistService waitlist)
    {
        _user = user;
        _waitlist = waitlist;
    }

    [HttpPost("join")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Join([FromBody] JoinWaitlistRequest request, CancellationToken ct)
    {
        await _waitlist.JoinAsync(_user.UserId, request.DoctorId, request.StartAtUtc, ct);
        return NoContent();
    }
}
