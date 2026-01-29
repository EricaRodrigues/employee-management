using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.DTOs;

public record EmployeeResponseDTO(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    EmployeeRoleEnum Role,
    Guid? ManagerId
);