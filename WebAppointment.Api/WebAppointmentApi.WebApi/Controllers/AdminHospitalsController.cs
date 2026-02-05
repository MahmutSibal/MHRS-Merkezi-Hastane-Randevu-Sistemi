using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Hospitals.Abstractions;
using WebAppointmentApi.Application.Hospitals.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/admin/hospitals")]
[Authorize(Roles = "Admin")]
public sealed class AdminHospitalsController : ControllerBase
{
    private readonly IHospitalService _hospitals;

    public AdminHospitalsController(IHospitalService hospitals)
    {
        _hospitals = hospitals;
    }

    [HttpGet]
    public Task<IReadOnlyList<HospitalDto>> List(CancellationToken ct)
        => _hospitals.ListAsync(ct);

    [HttpPost]
    public Task<HospitalDto> Create([FromBody] CreateHospitalRequest request, CancellationToken ct)
        => _hospitals.CreateAsync(request, ct);

    [HttpPost("{id:int}/assign-subadmin")]
    public Task<Guid> AssignSubAdmin([FromRoute] int id, [FromBody] AssignSubAdminRequest request, CancellationToken ct)
        => _hospitals.AssignSubAdminAsync(id, request.Email, request.Password, ct);

    [HttpGet("{id:int}/subadmins")]
    public Task<IReadOnlyList<SubAdminDto>> ListSubAdmins([FromRoute] int id, CancellationToken ct)
        => _hospitals.ListSubAdminsAsync(id, ct);

    [HttpPatch("{id:int}/subadmins/{subAdminUserId:guid}/credentials")]
    public async Task<IActionResult> UpdateSubAdminCredentials(
        [FromRoute] int id,
        [FromRoute] Guid subAdminUserId,
        [FromBody] UpdateSubAdminCredentialsRequest request,
        CancellationToken ct)
    {
        await _hospitals.UpdateSubAdminCredentialsAsync(id, subAdminUserId, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}/subadmins/{subAdminUserId:guid}")]
    public async Task<IActionResult> DeleteSubAdmin(
        [FromRoute] int id,
        [FromRoute] Guid subAdminUserId,
        CancellationToken ct)
    {
        await _hospitals.DeleteSubAdminAsync(id, subAdminUserId, ct);
        return NoContent();
    }
}
