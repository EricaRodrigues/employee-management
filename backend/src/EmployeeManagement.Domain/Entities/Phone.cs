namespace EmployeeManagement.Domain.Entities;

public class Phone
{
    public string Number { get; private set; }

    protected Phone() { } // EF

    public Phone(string number)
    {
        Number = number;
    }
}