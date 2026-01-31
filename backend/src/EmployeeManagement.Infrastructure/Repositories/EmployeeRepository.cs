using EmployeeManagement.Infrastructure.Context;
using EmployeeManagement.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Repositories;

// Handles employee database operations
public class EmployeeRepository(EmployeeDbContext context) : IEmployeeRepository
{
    // Get all employees
    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await context.Employees
            .Include(e => e.Phones)
            .Include(e => e.Manager)
            .AsNoTracking()
            .ToListAsync();
    }
    
    // Checks if document number already exists
    public async Task<bool> ExistsByDocumentAsync(string docNumber)
    {
        return await context.Employees
            .AnyAsync(e => e.DocNumber == docNumber);
    }
    
    // Checks if email already exists
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await context.Employees
            .AnyAsync(e => e.Email == email);
    }

    // Gets employee by id
    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        return await context.Employees
            .Include(e => e.Phones)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
    
    // Gets employee by email
    public async Task<Employee?> GetByEmailAsync(string email)
    {
        return await context.Employees
            .FirstOrDefaultAsync(e => e.Email == email);
    }

    // Saves new employee
    public async Task AddAsync(Employee employee)
    {
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
    }
    
    // Updates an employee
    public async Task UpdateAsync(Employee employee)
    {
        context.Employees.Update(employee);
        await context.SaveChangesAsync();
    }

    // Delete an employee
    public async Task DeleteAsync(Employee employee)
    {
        context.Employees.Remove(employee);
        await context.SaveChangesAsync();
    }
    
    // Check if manager has employee
    public async Task<bool> HasSubordinatesAsync(Guid managerId)
    {
        return await context.Employees
            .AnyAsync(e => e.ManagerId == managerId);
    }
}