using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;

public class Employee
{
    public Guid Id { get; private set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string DocNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public EmployeeRoleEnum Role { get; set; }

    public Guid? ManagerId { get; set; }
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
        EmployeeRoleEnum role,
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

    public void AddPhone(Phone phone)
    {
        Phones.Add(phone);
    }
}