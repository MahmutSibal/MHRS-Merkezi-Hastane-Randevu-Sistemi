using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Dependents.Abstractions;
using WebAppointmentApi.Application.Dependents.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/patient/dependents")]
[Authorize(Roles = "Patient")]
public sealed class PatientDependentsController : ControllerBase
{
    private readonly IUserContext _user;
    private readonly IDependentService _dependents;

    public PatientDependentsController(IUserContext user, IDependentService dependents)
    {
        _user = user;
        _dependents = dependents;
    }

    [HttpGet("me")]
    public Task<IReadOnlyList<DependentDto>> ListMy(CancellationToken ct)
        => _dependents.ListMyAsync(_user.UserId, ct);

    [HttpPost("me")]
    public Task<DependentDto> CreateMy([FromBody] CreateDependentRequest request, CancellationToken ct)
        => _dependents.CreateAsync(_user.UserId, request, ct);

    [HttpDelete("me/{dependentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteMy([FromRoute] int dependentId, CancellationToken ct)
    {
        await _dependents.DeleteAsync(_user.UserId, dependentId, ct);
        return NoContent();
    }
}
