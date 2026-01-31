using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.DTOs;

public record UpdateEmployeeRequestDTO(
    string FirstName,
    string LastName,
    string Email,
    string DocNumber,
    DateTime BirthDate,
    EmployeeRoleEnum Role,
    Guid? ManagerId,
    List<string> Phones
);