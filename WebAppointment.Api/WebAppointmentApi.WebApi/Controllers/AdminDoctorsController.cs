using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Doctors.Abstractions;
using WebAppointmentApi.Application.Doctors.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/admin/doctors")]
[Authorize(Roles = "Admin")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class AdminDoctorsController : ControllerBase
{
    private readonly IDoctorService _doctors;

    public AdminDoctorsController(IDoctorService doctors)
    {
        _doctors = doctors;
    }

    [HttpGet]
    public IActionResult List()
        => Forbid();

    [HttpPost]
    public IActionResult Create([FromBody] CreateDoctorRequest request)
        => Forbid();

    [HttpPut("{id:int}")]
    public IActionResult Update([FromRoute] int id, [FromBody] UpdateDoctorRequest request)
        => Forbid();

    [HttpDelete("{id:int}")]
    public IActionResult Delete([FromRoute] int id)
        => Forbid();
}
