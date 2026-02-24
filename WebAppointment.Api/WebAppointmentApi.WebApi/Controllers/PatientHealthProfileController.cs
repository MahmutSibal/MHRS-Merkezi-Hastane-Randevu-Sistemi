using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Abstractions;
using WebAppointmentApi.Application.Patients.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/patient/health-profile")]
[Authorize(Roles = "Patient")]
public sealed class PatientHealthProfileController : ControllerBase
{
    private readonly IPatientRepository _patients;
    private readonly IUserContext _user;

    public PatientHealthProfileController(IPatientRepository patients, IUserContext user)
    {
        _patients = patients;
        _user = user;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PatientHealthProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PatientHealthProfileDto>> Get(CancellationToken ct)
    {
        var patient = await _patients.FindByUserIdAsync(_user.UserId, ct);
        if (patient is null)
            return NotFound("Hasta profili bulunamadı.");

        return Ok(new PatientHealthProfileDto(
            patient.BloodType,
            patient.Allergies,
            patient.ChronicDiseases,
            patient.Medications,
            patient.EmergencyContactName,
            patient.EmergencyContactPhone));
    }

    [HttpPut]
    [ProducesResponseType(typeof(PatientHealthProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PatientHealthProfileDto>> Update([FromBody] UpdatePatientHealthProfileRequest request, CancellationToken ct)
    {
        var patient = await _patients.FindByUserIdAsync(_user.UserId, ct);
        if (patient is null)
            return NotFound("Hasta profili bulunamadı.");

        var validBloodTypes = new HashSet<string> { "A+", "A-", "B+", "B-", "AB+", "AB-", "0+", "0-" };
        if (!string.IsNullOrWhiteSpace(request.BloodType) && !validBloodTypes.Contains(request.BloodType.Trim()))
            return BadRequest("Geçersiz kan grubu. Geçerli değerler: A+, A-, B+, B-, AB+, AB-, 0+, 0-");

        patient.BloodType = request.BloodType?.Trim();
        patient.Allergies = request.Allergies?.Trim();
        patient.ChronicDiseases = request.ChronicDiseases?.Trim();
        patient.Medications = request.Medications?.Trim();
        patient.EmergencyContactName = request.EmergencyContactName?.Trim();
        patient.EmergencyContactPhone = request.EmergencyContactPhone?.Trim();

        await _patients.SaveChangesAsync(ct);

        return Ok(new PatientHealthProfileDto(
            patient.BloodType,
            patient.Allergies,
            patient.ChronicDiseases,
            patient.Medications,
            patient.EmergencyContactName,
            patient.EmergencyContactPhone));
    }
}
