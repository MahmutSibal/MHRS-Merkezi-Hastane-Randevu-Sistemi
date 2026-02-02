using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Departments.Abstractions;
using WebAppointmentApi.Application.Departments.Dtos;

namespace WebAppointmentApi.WebApi.Controllers;

[ApiController]
[Route("api/admin/departments")]
[Authorize(Roles = "Admin")]
public sealed class AdminDepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departments;

    public AdminDepartmentsController(IDepartmentService departments)
    {
        _departments = departments;
    }

    [HttpGet]
    public Task<IReadOnlyList<DepartmentDto>> List(CancellationToken ct)
        => _departments.ListAsync(ct);

    [HttpPost]
    public Task<DepartmentDto> Create([FromBody] CreateDepartmentRequest request, CancellationToken ct)
        => _departments.CreateAsync(request, ct);

    [HttpPut("{id:int}")]
    public Task<DepartmentDto> Update([FromRoute] int id, [FromBody] UpdateDepartmentRequest request, CancellationToken ct)
        => _departments.UpdateAsync(id, request, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        await _departments.DeleteAsync(id, ct);
        return NoContent();
    }
}
