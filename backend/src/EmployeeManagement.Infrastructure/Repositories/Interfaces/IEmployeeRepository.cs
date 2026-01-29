namespace EmployeeManagement.Infrastructure.Repositories.Interfaces;

public interface IEmployeeRepository
{
    Task<bool> ExistsByDocumentAsync(string documentNumber);
    Task<Employee?> GetByIdAsync(Guid id);
    Task AddAsync(Employee employee);
}