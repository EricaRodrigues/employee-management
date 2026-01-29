namespace EmployeeManagement.Infrastructure.Repositories.Interfaces;

public interface IEmployeeRepository
{
    Task<bool> ExistsByDocumentAsync(string docNumber);
    Task<Employee?> GetByEmailAsync(string email);
    Task<Employee?> GetByIdAsync(Guid id);
    Task AddAsync(Employee employee);
}