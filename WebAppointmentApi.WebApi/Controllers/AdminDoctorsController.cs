using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Doctors.Abstractions;
using WebAppointmentApi.Application.Doctors.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/admin/doctors")]
[Authorize(Roles = "Admin")]
public sealed class AdminDoctorsController : ControllerBase
{
    private readonly IDoctorService _doctors;

    public AdminDoctorsController(IDoctorService doctors)
    {
        _doctors = doctors;
    }

    [HttpGet]
    public Task<IReadOnlyList<DoctorDto>> List(CancellationToken ct)
        => _doctors.ListAsync(ct);

    [HttpPost]
    public Task<DoctorDto> Create([FromBody] CreateDoctorRequest request, CancellationToken ct)
        => _doctors.CreateAsync(request, ct);

    [HttpPut("{id:int}")]
    public Task<DoctorDto> Update([FromRoute] int id, [FromBody] UpdateDoctorRequest request, CancellationToken ct)
        => _doctors.UpdateAsync(id, request, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        await _doctors.DeleteAsync(id, ct);
        return NoContent();
    }
}
