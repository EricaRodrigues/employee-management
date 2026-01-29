using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.Application.Mappings;

public static class EmployeeMappingExtensions
{
    public static EmployeeResponseDTO ToResponse(this Employee employee)
    {
        return new EmployeeResponseDTO(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.Role,
            employee.ManagerId
        );
    }
}