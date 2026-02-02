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
}

public sealed record AssignSubAdminRequest(string Email, string Password);
