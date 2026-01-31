using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers;


[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // Authenticates user and returns JWT token
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDTO request)
    {
        var result = await authService.LoginAsync(request);
        return Ok(result);
    }
}