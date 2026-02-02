using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Patients.Abstractions;
using WebAppointmentApi.Application.Patients.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

/// <summary>
/// Admin patient management endpoints.
/// </summary>
[ApiController]
[Route("api/admin/patients")]
[Authorize(Roles = "Admin")]
public sealed class AdminPatientsController : ControllerBase
{
    private readonly IPatientService _patients;

    /// <summary>
    /// Creates a new instance of <see cref="AdminPatientsController"/>.
    /// </summary>
    public AdminPatientsController(IPatientService patients)
    {
        _patients = patients;
    }

    /// <summary>
    /// Creates a new patient account.
    /// </summary>
    /// <remarks>
    /// Includes TC Kimlik No validation and uniqueness enforcement.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    public Task<PatientDto> Create([FromBody] CreatePatientRequest request, CancellationToken ct)
        => _patients.CreateAsync(request, ct);

    /// <summary>
    /// Lists all non-deleted patients.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PatientDto>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<PatientDto>> List(CancellationToken ct)
        => _patients.ListAsync(ct);

    /// <summary>
    /// Updates an existing patient.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    public Task<PatientDto> Update([FromRoute] int id, [FromBody] UpdatePatientRequest request, CancellationToken ct)
        => _patients.UpdateAsync(id, request, ct);

    /// <summary>
    /// Soft-deletes a patient.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        await _patients.DeleteAsync(id, ct);
        return NoContent();
    }
}