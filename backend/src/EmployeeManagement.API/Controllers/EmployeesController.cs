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
    // Get all employees
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await employeeService.GetAllAsync();
        return Ok(result);
    }
    
    // Gets employee by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await employeeService.GetByIdAsync(id);
        return Ok(result);
    }
    
    // Creates a new employee
    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeRequestDTO request)
    {
        var currentEmployeeId = GetCurrentEmployeeId();

        var result = await employeeService.CreateAsync(request, currentEmployeeId);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    
    // Updates an employee
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateEmployeeRequestDTO request)
    {
        var currentEmployeeId = GetCurrentEmployeeId();

        var result = await employeeService.UpdateAsync(
            id,
            request,
            currentEmployeeId
        );

        return Ok(result);
    }
    
    // Deletes an employee
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var currentEmployeeId = GetCurrentEmployeeId();

        await employeeService.DeleteAsync(id, currentEmployeeId);

        return NoContent();
    }

    // Reads employee id from JWT claims
    private Guid GetCurrentEmployeeId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim == null ? Guid.Empty : Guid.Parse(claim.Value);
    }
}