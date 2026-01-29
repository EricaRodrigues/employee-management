using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController(IEmployeeService employeeService) : ControllerBase
{
    // Creates a new employee
    [Authorize(Roles = "Leader,Director")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeRequestDTO request)
    {
        var currentEmployeeId = GetCurrentEmployeeId();

        var result = await employeeService.CreateAsync(request, currentEmployeeId);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // Gets employee by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await employeeService.GetByIdAsync(id);
        return Ok(result);
    }

    // Reads employee id from JWT claims
    private Guid GetCurrentEmployeeId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim == null ? Guid.Empty : Guid.Parse(claim.Value);
    }
}