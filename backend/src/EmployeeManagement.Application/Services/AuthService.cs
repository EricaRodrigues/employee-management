using System.IdentityModel.Tokens.Jwt;
using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Services.Interfaces;
using EmployeeManagement.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagement.Application.Services;

// Handles authentication and JWT generation
public class AuthService(
    IEmployeeRepository employeeRepository,
    IConfiguration configuration,
    ILogger<AuthService> logger
) : IAuthService
{
    // Validates user credentials and returns a JWT token
    public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
    {
        var employee = await employeeRepository.GetByEmailAsync(request.Email);

        if (employee is null)
        {
            logger.LogWarning(
                "Invalid login attempt. Email not found: {Email}",
                request.Email
            );

            throw new BusinessException("Invalid credentials.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash))
        {
            logger.LogWarning(
                "Invalid login attempt. Wrong password for Email: {Email}",
                request.Email
            );

            throw new BusinessException("Invalid credentials.");
        }

        var token = GenerateToken(employee);
        
        logger.LogInformation(
            "User logged in successfully. EmployeeId: {EmployeeId}, Role: {Role}",
            employee.Id,
            employee.Role
        );

        return new LoginResponseDTO(token);
    }

    // Generates JWT token with employee claims
    private string GenerateToken(Employee employee)
    {
        // Claims used for authentication and authorization
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new Claim(ClaimTypes.Role, employee.Role.ToString())
        };

        // Create security key using secret from configuration
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
        );

        // Create signing credentials
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create JWT token
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        // Return token as string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}