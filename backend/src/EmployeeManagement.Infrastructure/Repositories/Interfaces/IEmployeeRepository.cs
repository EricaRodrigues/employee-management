namespace EmployeeManagement.Infrastructure.Repositories.Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<bool> ExistsByDocumentAsync(string docNumber);
    Task<bool> ExistsByEmailAsync(string email);
    Task<Employee?> GetByEmailAsync(string email);
    Task<Employee?> GetByIdAsync(Guid id);
    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(Employee employee);
    Task<bool> HasSubordinatesAsync(Guid managerId);
}