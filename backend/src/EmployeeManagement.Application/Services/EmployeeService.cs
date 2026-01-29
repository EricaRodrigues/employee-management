using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Mappings;
using EmployeeManagement.Application.Services.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Repositories.Interfaces;

namespace EmployeeManagement.Application.Services;

// Handles employee business rules
public class EmployeeService(IEmployeeRepository employeeRepository) : IEmployeeService
{
    // Get an employee by id
    public async Task<EmployeeResponseDTO> GetByIdAsync(Guid id)
    {
        var employee = await employeeRepository.GetByIdAsync(id);

        if (employee is null)
            throw new BusinessException("Employee not found.");

        return employee.ToResponse();
    }
    
    // Creates a new employee
    public async Task<EmployeeResponseDTO> CreateAsync(CreateEmployeeRequestDTO request, Guid currentEmployeeId)
    {
        // // Get current logged employee from database
        // var currentEmployee = await employeeRepository.GetByIdAsync(currentEmployeeId);
        // if (currentEmployee is null)
        //     throw new BusinessException("Current user not found.");
        
        // Validate business rules
        await ValidateBusinessRules(request);

        // Hash password using BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        var employee = new Employee(
            request.FirstName,
            request.LastName,
            request.Email,
            request.DocNumber,
            DateTime.SpecifyKind(request.BirthDate, DateTimeKind.Utc),
            request.Role,
            passwordHash,
            request.ManagerId
        );

        foreach (var phone in request.Phones)
        {
            employee.AddPhone(new Phone(phone));
        }

        await employeeRepository.AddAsync(employee);
    
        // Map entity to response
        return employee.ToResponse();
    }
    
    // Validates employee creation rules
    private async Task ValidateBusinessRules(CreateEmployeeRequestDTO request)
    {
        // Temporary rule: first employee can be created without a manager
        if (request.ManagerId.HasValue)
        {
            var manager = await employeeRepository.GetByIdAsync(request.ManagerId.Value);
            if (manager is null)
                throw new BusinessException("Manager not found.");
        }
        
        // Validate age
        if (!IsAdult(request.BirthDate))
            throw new BusinessException("Employee must be at least 18 years old.");
        
        // // Validate role permission
        // if (request.Role > currentEmployee.Role)
        //     throw new BusinessException(
        //         "You cannot create a user with higher permissions than yours."
        //     );
        
        // Validate unique document number
        if (await employeeRepository.ExistsByDocumentAsync(request.DocNumber))
            throw new BusinessException("Document number already exists.");
        
        // Validate manager
        if (request.ManagerId.HasValue)
        {
            var manager = await employeeRepository.GetByIdAsync(request.ManagerId.Value);

            if (manager is null)
                throw new BusinessException("Manager not found.");
        }
    }
    
    // Checks if employee is adult
    private static bool IsAdult(DateTime birthDate)
    {
        var age = DateTime.Today.Year - birthDate.Year;
        if (birthDate > DateTime.Today.AddYears(-age)) 
            age--; 
        
        return age >= 18;
    }
}