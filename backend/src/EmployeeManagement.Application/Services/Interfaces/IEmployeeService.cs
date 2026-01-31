using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.Application.Services.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeResponseDTO>> GetAllAsync();
    Task<EmployeeResponseDTO> GetByIdAsync(Guid id);
    Task<EmployeeResponseDTO> CreateAsync(CreateEmployeeRequestDTO request, Guid currentEmployeeId);
    Task<EmployeeResponseDTO> UpdateAsync(
        Guid employeeId,
        UpdateEmployeeRequestDTO request,
        Guid currentEmployeeId
    );
    Task DeleteAsync(Guid employeeId, Guid currentEmployeeId);
}