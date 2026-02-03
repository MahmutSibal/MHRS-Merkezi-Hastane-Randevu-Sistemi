using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Patients.Abstractions;
using WebAppointmentApi.Application.Patients.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/hospital/patients")]
[Authorize(Roles = "HospitalAdmin")]
public sealed class HospitalPatientsController : ControllerBase
{
    private readonly IPatientService _patients;

    public HospitalPatientsController(IPatientService patients)
    {
        _patients = patients;
    }

    [HttpGet]
    public Task<IReadOnlyList<PatientDto>> List(CancellationToken ct)
        => _patients.ListAsync(ct);
}
