using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Mappings;
using EmployeeManagement.Application.Services.Interfaces;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Application.Services;

// Handles employee business rules
public class EmployeeService(IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger) : IEmployeeService
{
    // Get all employees
    public async Task<IEnumerable<EmployeeResponseDTO>> GetAllAsync()
    {
        var employees = await employeeRepository.GetAllAsync();

        return employees.Select(e => e.ToResponse());
    }

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
        // Get current logged employee from database
        var currentEmployee = await employeeRepository.GetByIdAsync(currentEmployeeId);

        if (currentEmployee is null)
            throw new BusinessException("Current user not found.");

        // Validate business rules
        await ValidateBusinessRules(request, currentEmployee);

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
        
        logger.LogInformation(
            "Employee created. EmployeeId: {EmployeeId}, CreatedBy: {CurrentEmployeeId}, Role: {Role}",
            employee.Id,
            currentEmployeeId,
            employee.Role
        );

        // Map entity to response
        return employee.ToResponse();
    }

    // Validates employee creation rules
    private async Task ValidateBusinessRules(CreateEmployeeRequestDTO request, Employee currentEmployee)
    {
        // Validate age
        if (!IsAdult(request.BirthDate))
            throw new BusinessException("Employee must be at least 18 years old.");

        // Validate role permission
        if (request.Role > currentEmployee.Role)
            throw new BusinessException(
                "You cannot create a user with higher permissions than yours."
            );

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

    public async Task<EmployeeResponseDTO> UpdateAsync(
        Guid employeeId,
        UpdateEmployeeRequestDTO request,
        Guid currentEmployeeId)
    {
        var employee = await employeeRepository.GetByIdAsync(employeeId);

        if (employee is null)
            throw new BusinessException("Employee not found.");

        var currentEmployee = await employeeRepository.GetByIdAsync(currentEmployeeId);

        if (currentEmployee is null)
            throw new BusinessException("Current user not found.");

        // Age validation
        if (!IsAdult(request.BirthDate))
            throw new BusinessException("Employee must be at least 18 years old.");

        // Role rules
        if (employee.Id == currentEmployee.Id && request.Role != employee.Role)
            throw new BusinessException("You cannot change your own role.");

        if (request.Role > currentEmployee.Role)
            throw new BusinessException(
                "You cannot assign a role higher than yours."
            );

        // DocNumber uniqueness
        if (employee.DocNumber != request.DocNumber &&
            await employeeRepository.ExistsByDocumentAsync(request.DocNumber))
            throw new BusinessException("Document number already exists.");

        // Manager validation
        if (request.ManagerId.HasValue)
        {
            if (request.ManagerId == employee.Id)
                throw new BusinessException("Employee cannot be their own manager.");

            var manager = await employeeRepository.GetByIdAsync(request.ManagerId.Value);

            if (manager is null)
                throw new BusinessException("Manager not found.");
        }

        // Apply updates
        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.DocNumber = request.DocNumber;
        employee.BirthDate = DateTime.SpecifyKind(request.BirthDate, DateTimeKind.Utc);
        employee.Role = request.Role;
        employee.ManagerId = request.ManagerId;
        
        employee.Phones.Clear();

        foreach (var phone in request.Phones)
            employee.AddPhone(new Phone(phone));

        await employeeRepository.UpdateAsync(employee);
        
        logger.LogInformation(
            "Employee updated. EmployeeId: {EmployeeId}, UpdatedBy: {CurrentEmployeeId}",
            employee.Id,
            currentEmployeeId
        );

        return employee.ToResponse();
    }

    // Deletes an employee respecting role hierarchy
    public async Task DeleteAsync(Guid employeeId, Guid currentEmployeeId)
    {
        if (employeeId == currentEmployeeId)
            throw new BusinessException("You cannot delete yourself.");

        var currentEmployee = await employeeRepository.GetByIdAsync(currentEmployeeId);

        if (currentEmployee is null)
            throw new BusinessException("Current user not found.");

        var employeeToDelete = await employeeRepository.GetByIdAsync(employeeId);

        if (employeeToDelete is null)
            throw new BusinessException("Employee not found.");

        // Authorization rules
        switch (currentEmployee.Role)
        {
            case EmployeeRoleEnum.Employee:
                throw new BusinessException("You are not allowed to delete users.");

            case EmployeeRoleEnum.Leader:
                if (employeeToDelete.Role != EmployeeRoleEnum.Employee)
                    throw new BusinessException("You cannot delete a user with equal or higher permissions.");
                break;

            case EmployeeRoleEnum.Director:
                // Director can delete any user except himself
                break;
        }

        await employeeRepository.DeleteAsync(employeeToDelete);
        
        logger.LogWarning(
            "Employee deleted. EmployeeId: {EmployeeId}, DeletedBy: {CurrentEmployeeId}, DeletedRole: {Role}",
            employeeToDelete.Id,
            currentEmployeeId,
            employeeToDelete.Role
        );
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