using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Repositories.Interfaces;
using FluentAssertions;
using Moq;

namespace EmployeeManagement.Tests.Services;

public class EmployeeServiceTests
{
    // ----------------------------
    // Helpers
    // ----------------------------
    private static Employee CreateCurrentEmployee()
    {
        return new Employee(
            firstName: "Current",
            lastName: "User",
            email: "current@company.com",
            docNumber: "999999999",
            birthDate: DateTime.UtcNow.AddYears(-30),
            role: EmployeeRoleEnum.Director,
            passwordHash: "hashed-password"
        );
    }
    
    [Fact]
    public async Task CreateAsync_ShouldCreateEmployee_WhenEverthingIsValid()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        repositoryMock
            .Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Employee(
                "Manager",
                "User",
                "manager@company.com",
                "999999999",
                DateTime.UtcNow.AddYears(-30),
                EmployeeRoleEnum.Leader,
                "hashed-password"
            ));

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Employee>()))
            .Returns(Task.CompletedTask);

        var service = new EmployeeService(repositoryMock.Object);

        var request = new CreateEmployeeRequestDTO(
            FirstName: "Erica",
            LastName: "Rodrigues",
            Email: "erica@company.com",
            DocNumber: "123456789",
            BirthDate: DateTime.UtcNow.AddYears(-25),
            Role: EmployeeRoleEnum.Employee,
            ManagerId: Guid.NewGuid(),
            Password: "password123",
            Phones: ["11999999999"]
        );

        var result = await service.CreateAsync(request, Guid.NewGuid());

        result.Should().NotBeNull();
        result.FirstName.Should().Be("Erica");
        result.LastName.Should().Be("Rodrigues");
        result.Email.Should().Be("erica@company.com");
        result.Role.Should().Be(EmployeeRoleEnum.Employee);

        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Employee>()),
            Times.Once
        );
    }
    
    [Fact]
    public async Task CreateAsync_ShouldCreateEmployee_WhenNoPhoneIsProvided()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        repositoryMock
            .Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var currentEmployee = CreateCurrentEmployee();
        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(currentEmployee);

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Employee>()))
            .Returns(Task.CompletedTask);

        var service = new EmployeeService(repositoryMock.Object);

        var request = new CreateEmployeeRequestDTO(
            FirstName: "Erica",
            LastName: "Rodrigues",
            Email: "erica@company.com",
            DocNumber: "123456789",
            BirthDate: DateTime.UtcNow.AddYears(-25),
            Role: EmployeeRoleEnum.Employee,
            ManagerId: null,
            Password: "password123",
            Phones: [] 
        );

        var result = await service.CreateAsync(request, Guid.NewGuid());

        result.Should().NotBeNull();
        result.FirstName.Should().Be("Erica");
        result.LastName.Should().Be("Rodrigues");
        result.Email.Should().Be("erica@company.com");

        repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Employee>()),
            Times.Once
        );
    }
    
    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenEmployeeIsNotMinor()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        repositoryMock
            .Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var currentEmployee = CreateCurrentEmployee();
        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(currentEmployee);

        var service = new EmployeeService(repositoryMock.Object);

        var request = new CreateEmployeeRequestDTO(
            FirstName: "Erica",
            LastName: "Rodrigues",
            Email: "erica@company.com",
            DocNumber: "123456789",
            BirthDate: DateTime.UtcNow.AddYears(-17),
            Role: EmployeeRoleEnum.Employee,
            ManagerId: null,
            Password: "password123",
            Phones: new List<string>()
        );

        Func<Task> act = async () =>
            await service.CreateAsync(request, Guid.NewGuid());

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Employee must be at least 18 years old.");
    }
    
    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenDocumentAlreadyExists()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var currentEmployee = CreateCurrentEmployee();
        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(currentEmployee);
        
        repositoryMock
            .Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var service = new EmployeeService(repositoryMock.Object);

        var request = new CreateEmployeeRequestDTO(
            "Erica",
            "Rodrigues",
            "erica@company.com",
            "123456789",
            DateTime.UtcNow.AddYears(-25),
            EmployeeRoleEnum.Employee,
            null,
            "password123",
            new List<string>()
        );

        Func<Task> act = async () =>
            await service.CreateAsync(request, Guid.NewGuid());

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Document number already exists.");
    }
    
    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenManagerDoesNotExist()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();
        repositoryMock
            .Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var currentEmployee = CreateCurrentEmployee();
        var managerId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == currentEmployee.Id)))
            .ReturnsAsync(currentEmployee);

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == managerId)))
            .ReturnsAsync((Employee?)null);

        var service = new EmployeeService(repositoryMock.Object);

        var request = new CreateEmployeeRequestDTO(
            FirstName: "Erica",
            LastName: "Rodrigues",
            Email: "erica@company.com",
            DocNumber: "123456789",
            BirthDate: DateTime.UtcNow.AddYears(-25),
            Role: EmployeeRoleEnum.Employee,
            ManagerId: managerId,
            Password: "password123",
            Phones: new List<string>()
        );

        Func<Task> act = async () =>
            await service.CreateAsync(request, currentEmployee.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Manager not found.");
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldThrowException_WhenEmployeeDoesNotExist()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Employee?)null);

        var service = new EmployeeService(repositoryMock.Object);

        Func<Task> act = async () =>
            await service.GetByIdAsync(Guid.NewGuid());

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Employee not found.");
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnEmployee_WhenEmployeeExists()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var employee = new Employee(
            firstName: "Erica",
            lastName: "Rodrigues",
            email: "erica@company.com",
            docNumber: "123456789",
            birthDate: DateTime.UtcNow.AddYears(-30),
            role: EmployeeRoleEnum.Employee,
            passwordHash: "hashed-password"
        );

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(employee);

        var service = new EmployeeService(repositoryMock.Object);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        result.Should().NotBeNull();
        result.FirstName.Should().Be("Erica");
        result.LastName.Should().Be("Rodrigues");
        result.Email.Should().Be("erica@company.com");
        result.Role.Should().Be(EmployeeRoleEnum.Employee);
    }
    
    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenCreatingUserWithHigherRole()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var currentEmployee = new Employee(
            "Leader",
            "User",
            "leader@company.com",
            "999999999",
            DateTime.UtcNow.AddYears(-30),
            EmployeeRoleEnum.Leader,
            "hashed-password"
        );

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(currentEmployee);

        repositoryMock
            .Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var service = new EmployeeService(repositoryMock.Object);

        var request = new CreateEmployeeRequestDTO(
            "New",
            "Director",
            "director@company.com",
            "123456789",
            DateTime.UtcNow.AddYears(-40),
            EmployeeRoleEnum.Director,
            null,
            "password123",
            new List<string>()
        );

        Func<Task> act = async () =>
            await service.CreateAsync(request, currentEmployee.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("You cannot create a user with higher permissions than yours.");
    }
    
    
}