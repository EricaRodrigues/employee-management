namespace EmployeeManagement.Application.DTOs;

public record LoginRequestDTO(
    string Email,
    string Password
);