using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;

public class Employee
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string DocNumber { get; private set; }

    public DateTime BirthDate { get; private set; }

    public EmployeeRole Role { get; private set; }

    public Guid? ManagerId { get; private set; }
    public Employee? Manager { get; private set; }

    public ICollection<Phone> Phones { get; private set; } = new List<Phone>();

    public string PasswordHash { get; private set; }

    protected Employee() { } // EF

    public Employee(
        string firstName,
        string lastName,
        string email,
        string docNumber,
        DateTime birthDate,
        EmployeeRole role,
        string passwordHash,
        Guid? managerId = null
    )
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        DocNumber = docNumber;
        BirthDate = birthDate;
        Role = role;
        PasswordHash = passwordHash;
        ManagerId = managerId;
    }

    public void ChangeRole(EmployeeRole role)
    {
        Role = role;
    }

    public void AddPhone(Phone phone)
    {
        Phones.Add(phone);
    }
}