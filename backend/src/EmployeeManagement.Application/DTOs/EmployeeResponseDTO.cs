using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.DTOs;

public record EmployeeResponseDTO(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string DocNumber,
    DateTime BirthDate,
    EmployeeRoleEnum Role,
    Guid? ManagerId,
    List<string> Phones
);