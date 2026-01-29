using EmployeeManagement.Infrastructure.Context;
using EmployeeManagement.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Repositories;

// Handles employee database operations
public class EmployeeRepository(EmployeeDbContext context) : IEmployeeRepository
{
    // Checks if document number already exists
    public async Task<bool> ExistsByDocumentAsync(string docNumber)
    {
        return await context.Employees
            .AnyAsync(e => e.DocNumber == docNumber);
    }

    // Gets employee by id
    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        return await context.Employees
            .Include(e => e.Phones)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    // Saves new employee
    public async Task AddAsync(Employee employee)
    {
        context.Employees.Add(employee);
        await context.SaveChangesAsync();
    }
}