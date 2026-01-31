using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.Application.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request);
}