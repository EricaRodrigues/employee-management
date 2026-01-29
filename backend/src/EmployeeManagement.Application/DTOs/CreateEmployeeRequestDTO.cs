using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.DTOs;

public record CreateEmployeeRequestDTO(
    string FirstName,
    string LastName,
    string Email,
    string DocNumber,
    DateTime BirthDate,
    EmployeeRoleEnum Role,
    Guid? ManagerId,
    string Password,
    List<string> Phones
);