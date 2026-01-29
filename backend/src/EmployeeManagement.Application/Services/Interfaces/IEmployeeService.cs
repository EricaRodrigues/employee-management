using EmployeeManagement.Application.DTOs;

namespace EmployeeManagement.Application.Services.Interfaces;

public interface IEmployeeService
{
    Task<EmployeeResponseDTO> GetByIdAsync(Guid id);
    Task<EmployeeResponseDTO> CreateAsync(CreateEmployeeRequestDTO request, Guid currentEmployeeId);
}