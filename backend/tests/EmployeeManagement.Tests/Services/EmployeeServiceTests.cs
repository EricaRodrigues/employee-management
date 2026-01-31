using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Repositories.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeManagement.Tests.Services;

public class EmployeeServiceTests
{
    // ----------------------------
    // Helpers
    // ----------------------------
    private static Employee CreateEmployee(EmployeeRoleEnum role)
    {
        return new Employee(
            firstName: "Current",
            lastName: "User",
            email: $"{Guid.NewGuid()}@company.com",
            docNumber: "999999999",
            birthDate: DateTime.UtcNow.AddYears(-30),
            role: role,
            passwordHash: "hashed-password"
        );
    }
    
    private static EmployeeService CreateService(
        Mock<IEmployeeRepository> repositoryMock
    )
    {
        var loggerMock = new Mock<ILogger<EmployeeService>>();
        return new EmployeeService(repositoryMock.Object, loggerMock.Object);
    }
    
    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoEmployeesExist()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Employee>());

        var service = CreateService(repositoryMock);

        var result = await service.GetAllAsync();

        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetAllAsync_ShouldReturnEmployees_WhenEmployeesExist()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var employees = new List<Employee>
        {
            new Employee(
                "Erica",
                "Rodrigues",
                "erica@company.com",
                "123",
                DateTime.UtcNow.AddYears(-30),
                EmployeeRoleEnum.Employee,
                "hash"
            ),
            new Employee(
                "Danilo",
                "Rodrigues",
                "danilo@company.com",
                "456",
                DateTime.UtcNow.AddYears(-28),
                EmployeeRoleEnum.Leader,
                "hash"
            )
        };

        repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(employees);

        var service = CreateService(repositoryMock);

        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldThrowException_WhenEmployeeDoesNotExist()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Employee?)null);

        var service = CreateService(repositoryMock);

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

        var service = CreateService(repositoryMock);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        result.Should().NotBeNull();
        result.FirstName.Should().Be("Erica");
        result.LastName.Should().Be("Rodrigues");
        result.Email.Should().Be("erica@company.com");
        result.Role.Should().Be(EmployeeRoleEnum.Employee);
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

        var service = CreateService(repositoryMock);

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

        var currentEmployee = CreateEmployee(EmployeeRoleEnum.Director);
        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(currentEmployee);

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Employee>()))
            .Returns(Task.CompletedTask);

        var service = CreateService(repositoryMock);

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

        var currentEmployee = CreateEmployee(EmployeeRoleEnum.Director);
        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(currentEmployee);

        var service = CreateService(repositoryMock);

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
    public async Task CreateAsync_ShouldThrowException_WhenDocNumberAlreadyExists()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var currentEmployee = CreateEmployee(EmployeeRoleEnum.Director);
        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(currentEmployee);
        
        repositoryMock
            .Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var service = CreateService(repositoryMock);

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
    public async Task CreateAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var currentEmployee = CreateEmployee(EmployeeRoleEnum.Director);

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(currentEmployee);

        repositoryMock
            .Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        repositoryMock
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var service = CreateService(repositoryMock);

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

        Func<Task> act = async () =>
            await service.CreateAsync(request, Guid.NewGuid());

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Email already exists.");
    }
    
    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenManagerDoesNotExist()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();
        repositoryMock
            .Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var currentEmployee = CreateEmployee(EmployeeRoleEnum.Director);
        var managerId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == currentEmployee.Id)))
            .ReturnsAsync(currentEmployee);

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == managerId)))
            .ReturnsAsync((Employee?)null);

        var service = CreateService(repositoryMock);

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

        var service = CreateService(repositoryMock);

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
            .WithMessage("Leaders can only create employees.");
    }
    
    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenManagerHasSameOrLowerRole()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var currentEmployee = CreateEmployee(EmployeeRoleEnum.Director);
        var manager = CreateEmployee(EmployeeRoleEnum.Employee);

        repositoryMock
            .Setup(r => r.GetByIdAsync(currentEmployee.Id))
            .ReturnsAsync(currentEmployee);

        repositoryMock
            .Setup(r => r.GetByIdAsync(manager.Id))
            .ReturnsAsync(manager);

        repositoryMock
            .Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        repositoryMock
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var service = CreateService(repositoryMock);

        var request = new CreateEmployeeRequestDTO(
            "New",
            "User",
            "new@company.com",
            "123456789",
            DateTime.UtcNow.AddYears(-25),
            EmployeeRoleEnum.Leader,
            manager.Id,
            "password123",
            []
        );

        Func<Task> act = async () =>
            await service.CreateAsync(request, currentEmployee.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Employee cannot be a manager.");
    }
    
    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenEmployeeTriesToCreateEmployee()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var employee = CreateEmployee(EmployeeRoleEnum.Employee);

        repositoryMock
            .Setup(r => r.GetByIdAsync(employee.Id))
            .ReturnsAsync(employee);

        var service = CreateService(repositoryMock);

        var request = new CreateEmployeeRequestDTO(
            "New",
            "Employee",
            "new@company.com",
            "123456789",
            DateTime.UtcNow.AddYears(-25),
            EmployeeRoleEnum.Employee,
            null,
            "password123",
            []
        );

        Func<Task> act = async () =>
            await service.CreateAsync(request, employee.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("You are not allowed to create users.");
    }
    
    [Fact]
    public async Task CreateAsync_ShouldCreateEmployee_WhenLeaderCreatesEmployee()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var leader = CreateEmployee(EmployeeRoleEnum.Leader);

        repositoryMock.Setup(r => r.GetByIdAsync(leader.Id)).ReturnsAsync(leader);
        repositoryMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>())).ReturnsAsync(false);
        repositoryMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
        repositoryMock.Setup(r => r.AddAsync(It.IsAny<Employee>())).Returns(Task.CompletedTask);

        var service = CreateService(repositoryMock);

        var request = new CreateEmployeeRequestDTO(
            "Employee",
            "User",
            "employee@company.com",
            "123456789",
            DateTime.UtcNow.AddYears(-25),
            EmployeeRoleEnum.Employee,
            null,
            "password123",
            []
        );

        var result = await service.CreateAsync(request, leader.Id);

        result.Role.Should().Be(EmployeeRoleEnum.Employee);
    }
    
    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenLeaderTriesToCreateLeader()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var leader = CreateEmployee(EmployeeRoleEnum.Leader);

        repositoryMock
            .Setup(r => r.GetByIdAsync(leader.Id))
            .ReturnsAsync(leader);

        var service = CreateService(repositoryMock);

        var request = new CreateEmployeeRequestDTO(
            "New",
            "Leader",
            "leader2@company.com",
            "123456789",
            DateTime.UtcNow.AddYears(-30),
            EmployeeRoleEnum.Leader,
            null,
            "password123",
            []
        );

        Func<Task> act = async () =>
            await service.CreateAsync(request, leader.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Leaders can only create employees.");
    }
    
    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenEmployeeIsManager()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var director = CreateEmployee(EmployeeRoleEnum.Director);
        var employeeManager = CreateEmployee(EmployeeRoleEnum.Employee);

        repositoryMock.Setup(r => r.GetByIdAsync(director.Id)).ReturnsAsync(director);
        repositoryMock.Setup(r => r.GetByIdAsync(employeeManager.Id)).ReturnsAsync(employeeManager);
        repositoryMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>())).ReturnsAsync(false);
        repositoryMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);

        var service = CreateService(repositoryMock);

        var request = new CreateEmployeeRequestDTO(
            "New",
            "User",
            "new@company.com",
            "123456789",
            DateTime.UtcNow.AddYears(-25),
            EmployeeRoleEnum.Employee,
            employeeManager.Id,
            "password123",
            []
        );

        Func<Task> act = async () =>
            await service.CreateAsync(request, director.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Employee cannot be a manager.");
    }
    
    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenLeaderManagesLeader()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var director = CreateEmployee(EmployeeRoleEnum.Director);
        var leaderManager = CreateEmployee(EmployeeRoleEnum.Leader);

        repositoryMock.Setup(r => r.GetByIdAsync(director.Id)).ReturnsAsync(director);
        repositoryMock.Setup(r => r.GetByIdAsync(leaderManager.Id)).ReturnsAsync(leaderManager);
        repositoryMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>())).ReturnsAsync(false);
        repositoryMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);

        var service = CreateService(repositoryMock);

        var request = new CreateEmployeeRequestDTO(
            "New",
            "Leader",
            "leader@company.com",
            "123456789",
            DateTime.UtcNow.AddYears(-30),
            EmployeeRoleEnum.Leader,
            leaderManager.Id,
            "password123",
            []
        );

        Func<Task> act = async () =>
            await service.CreateAsync(request, director.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Leader can only manage employees.");
    }
    
    [Fact]
    public async Task CreateAsync_ShouldCreateEmployee_WhenDirectorIsManager()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var director = CreateEmployee(EmployeeRoleEnum.Director);

        repositoryMock.Setup(r => r.GetByIdAsync(director.Id)).ReturnsAsync(director);
        repositoryMock.Setup(r => r.GetByIdAsync(director.Id)).ReturnsAsync(director);
        repositoryMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>())).ReturnsAsync(false);
        repositoryMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
        repositoryMock.Setup(r => r.AddAsync(It.IsAny<Employee>())).Returns(Task.CompletedTask);

        var service = CreateService(repositoryMock);

        var request = new CreateEmployeeRequestDTO(
            "Employee",
            "User",
            "employee@company.com",
            "123456789",
            DateTime.UtcNow.AddYears(-25),
            EmployeeRoleEnum.Employee,
            director.Id,
            "password123",
            []
        );

        var result = await service.CreateAsync(request, director.Id);

        result.ManagerId.Should().Be(director.Id);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldUpdateEmployee_WhenValid()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var current = CreateEmployee(EmployeeRoleEnum.Director);
        var employee = CreateEmployee(EmployeeRoleEnum.Employee);

        repositoryMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);
        repositoryMock.Setup(r => r.GetByIdAsync(current.Id)).ReturnsAsync(current);
        repositoryMock.Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>())).ReturnsAsync(false);
        repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>()))
            .Returns(Task.CompletedTask);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            "New",
            "Name",
            "new@company.com",
            "222222222",
            DateTime.UtcNow.AddYears(-25),
            EmployeeRoleEnum.Employee,
            null,
            []
        );

        var result = await service.UpdateAsync(employee.Id, request, current.Id);

        result.FirstName.Should().Be("New");
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldFail_WhenRoleIsHigherThanCurrent()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var leader = CreateEmployee(EmployeeRoleEnum.Leader);
        var employee = CreateEmployee(EmployeeRoleEnum.Employee);

        repositoryMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);
        repositoryMock.Setup(r => r.GetByIdAsync(leader.Id)).ReturnsAsync(leader);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.DocNumber,
            employee.BirthDate,
            EmployeeRoleEnum.Director,
            null,
            []
        );

        Func<Task> act = async () =>
            await service.UpdateAsync(employee.Id, request, leader.Id);

        await act.Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("You cannot assign a role higher than yours.");
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldFail_WhenSelfRoleChange()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var director = CreateEmployee(EmployeeRoleEnum.Director);

        repositoryMock.Setup(r => r.GetByIdAsync(director.Id)).ReturnsAsync(director);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            director.FirstName,
            director.LastName,
            director.Email,
            director.DocNumber,
            director.BirthDate,
            EmployeeRoleEnum.Leader,
            null,
            []
        );

        Func<Task> act = async () =>
            await service.UpdateAsync(director.Id, request, director.Id);

        await act.Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("You cannot change your own role.");
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenDocNumberAlreadyExists()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var current = CreateEmployee(EmployeeRoleEnum.Director);
        var employee = CreateEmployee(EmployeeRoleEnum.Employee);

        repositoryMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);
        repositoryMock.Setup(r => r.GetByIdAsync(current.Id)).ReturnsAsync(current);

        repositoryMock
            .Setup(r => r.ExistsByDocumentAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            employee.FirstName,
            employee.LastName,
            employee.Email,
            "DUPLICATE_DOC",
            employee.BirthDate,
            employee.Role,
            null,
            []
        );

        Func<Task> act = async () =>
            await service.UpdateAsync(employee.Id, request, current.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Document number already exists.");
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenEmailAlreadyInUse()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var current = CreateEmployee(EmployeeRoleEnum.Director);
        var employee = CreateEmployee(EmployeeRoleEnum.Employee);

        repositoryMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);
        repositoryMock.Setup(r => r.GetByIdAsync(current.Id)).ReturnsAsync(current);

        repositoryMock
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            employee.FirstName,
            employee.LastName,
            "duplicate@company.com",
            employee.DocNumber,
            employee.BirthDate,
            employee.Role,
            null,
            []
        );

        Func<Task> act = async () =>
            await service.UpdateAsync(employee.Id, request, current.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Email already in use.");
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenEmployeeIsOwnManager()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var current = CreateEmployee(EmployeeRoleEnum.Director);

        repositoryMock
            .Setup(r => r.GetByIdAsync(current.Id))
            .ReturnsAsync(current);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            current.FirstName,
            current.LastName,
            current.Email,
            current.DocNumber,
            current.BirthDate,
            current.Role,
            current.Id, // Same employee as manager
            []
        );

        Func<Task> act = async () =>
            await service.UpdateAsync(current.Id, request, current.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Employee cannot be their own manager.");
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenManagerDoesNotExist()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var current = CreateEmployee(EmployeeRoleEnum.Director);
        var managerId = Guid.NewGuid();

        repositoryMock
            .Setup(r => r.GetByIdAsync(current.Id))
            .ReturnsAsync(current);

        repositoryMock
            .Setup(r => r.GetByIdAsync(managerId))
            .ReturnsAsync((Employee?)null);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            current.FirstName,
            current.LastName,
            current.Email,
            current.DocNumber,
            current.BirthDate,
            current.Role,
            managerId,
            []
        );

        Func<Task> act = async () =>
            await service.UpdateAsync(current.Id, request, current.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Manager not found.");
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenManagerHasSameRole()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var employee = CreateEmployee(EmployeeRoleEnum.Leader);
        var manager = CreateEmployee(EmployeeRoleEnum.Leader);

        repositoryMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);
        repositoryMock.Setup(r => r.GetByIdAsync(manager.Id)).ReturnsAsync(manager);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.DocNumber,
            employee.BirthDate,
            employee.Role,
            manager.Id,
            []
        );

        Func<Task> act = async () =>
            await service.UpdateAsync(employee.Id, request, employee.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Manager must have a higher role.");
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenManagerHasLowerRole()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var employee = CreateEmployee(EmployeeRoleEnum.Director);
        var manager = CreateEmployee(EmployeeRoleEnum.Leader);

        repositoryMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);
        repositoryMock.Setup(r => r.GetByIdAsync(manager.Id)).ReturnsAsync(manager);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.DocNumber,
            employee.BirthDate,
            employee.Role,
            manager.Id,
            []
        );

        Func<Task> act = async () =>
            await service.UpdateAsync(employee.Id, request, employee.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Manager must have a higher role.");
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldUpdateManager_WhenManagerHasHigherRole()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var employee = CreateEmployee(EmployeeRoleEnum.Employee);
        var manager = CreateEmployee(EmployeeRoleEnum.Leader);

        repositoryMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);
        repositoryMock.Setup(r => r.GetByIdAsync(manager.Id)).ReturnsAsync(manager);
        repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>()))
            .Returns(Task.CompletedTask);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.DocNumber,
            employee.BirthDate,
            employee.Role,
            manager.Id,
            []
        );

        var result = await service.UpdateAsync(employee.Id, request, employee.Id);

        result.ManagerId.Should().Be(manager.Id);
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenEmployeeEditsAnotherEmployee()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var current = CreateEmployee(EmployeeRoleEnum.Employee);
        var target = CreateEmployee(EmployeeRoleEnum.Employee);

        repositoryMock.Setup(r => r.GetByIdAsync(current.Id)).ReturnsAsync(current);
        repositoryMock.Setup(r => r.GetByIdAsync(target.Id)).ReturnsAsync(target);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            target.FirstName,
            target.LastName,
            target.Email,
            target.DocNumber,
            target.BirthDate,
            target.Role,
            null,
            []
        );

        Func<Task> act = async () =>
            await service.UpdateAsync(target.Id, request, current.Id);

        await act.Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("You are not allowed to edit other users.");
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenLeaderEditsLeader()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var leader = CreateEmployee(EmployeeRoleEnum.Leader);
        var otherLeader = CreateEmployee(EmployeeRoleEnum.Leader);

        repositoryMock.Setup(r => r.GetByIdAsync(leader.Id)).ReturnsAsync(leader);
        repositoryMock.Setup(r => r.GetByIdAsync(otherLeader.Id)).ReturnsAsync(otherLeader);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            otherLeader.FirstName,
            otherLeader.LastName,
            otherLeader.Email,
            otherLeader.DocNumber,
            otherLeader.BirthDate,
            otherLeader.Role,
            null,
            []
        );

        Func<Task> act = async () =>
            await service.UpdateAsync(otherLeader.Id, request, leader.Id);

        await act.Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("You can only edit employees.");
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenLeaderEditsDirector()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var leader = CreateEmployee(EmployeeRoleEnum.Leader);
        var director = CreateEmployee(EmployeeRoleEnum.Director);

        repositoryMock.Setup(r => r.GetByIdAsync(leader.Id)).ReturnsAsync(leader);
        repositoryMock.Setup(r => r.GetByIdAsync(director.Id)).ReturnsAsync(director);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            director.FirstName,
            director.LastName,
            director.Email,
            director.DocNumber,
            director.BirthDate,
            director.Role,
            null,
            []
        );

        Func<Task> act = async () =>
            await service.UpdateAsync(director.Id, request, leader.Id);

        await act.Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("You can only edit employees.");
    }
    
    [Fact]
    public async Task UpdateAsync_ShouldAllowDirectorToEditLeader()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var director = CreateEmployee(EmployeeRoleEnum.Director);
        var leader = CreateEmployee(EmployeeRoleEnum.Leader);

        repositoryMock.Setup(r => r.GetByIdAsync(director.Id)).ReturnsAsync(director);
        repositoryMock.Setup(r => r.GetByIdAsync(leader.Id)).ReturnsAsync(leader);
        repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Employee>()))
            .Returns(Task.CompletedTask);

        var service = CreateService(repositoryMock);

        var request = new UpdateEmployeeRequestDTO(
            "Updated",
            leader.LastName,
            leader.Email,
            leader.DocNumber,
            leader.BirthDate,
            leader.Role,
            null,
            []
        );

        var result = await service.UpdateAsync(leader.Id, request, director.Id);

        result.FirstName.Should().Be("Updated");
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenDeletingYourself()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();
        var service = CreateService(repositoryMock);

        var id = Guid.NewGuid();

        Func<Task> act = async () =>
            await service.DeleteAsync(id, id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("You cannot delete yourself.");
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenCurrentUserNotFound()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Employee?)null);

        var service = CreateService(repositoryMock);

        Func<Task> act = async () =>
            await service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Current user not found.");
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenEmployeeDoesNotExist()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var currentEmployee = CreateEmployee(EmployeeRoleEnum.Director);

        repositoryMock
            .Setup(r => r.GetByIdAsync(currentEmployee.Id))
            .ReturnsAsync(currentEmployee);

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id != currentEmployee.Id)))
            .ReturnsAsync((Employee?)null);

        var service = CreateService(repositoryMock);

        Func<Task> act = async () =>
            await service.DeleteAsync(Guid.NewGuid(), currentEmployee.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Employee not found.");
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenDeletingUserWithSameRole()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var currentEmployee = CreateEmployee(EmployeeRoleEnum.Leader);
        var targetEmployee = CreateEmployee(EmployeeRoleEnum.Leader);

        repositoryMock
            .Setup(r => r.GetByIdAsync(currentEmployee.Id))
            .ReturnsAsync(currentEmployee);

        repositoryMock
            .Setup(r => r.GetByIdAsync(targetEmployee.Id))
            .ReturnsAsync(targetEmployee);

        var service = CreateService(repositoryMock);

        Func<Task> act = async () =>
            await service.DeleteAsync(targetEmployee.Id, currentEmployee.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("You cannot delete a user with equal or higher permissions.");
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenDeletingUserWithHigherRole()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var currentEmployee = CreateEmployee(EmployeeRoleEnum.Leader);
        var targetEmployee = CreateEmployee(EmployeeRoleEnum.Director);

        repositoryMock
            .Setup(r => r.GetByIdAsync(currentEmployee.Id))
            .ReturnsAsync(currentEmployee);

        repositoryMock
            .Setup(r => r.GetByIdAsync(targetEmployee.Id))
            .ReturnsAsync(targetEmployee);

        var service = CreateService(repositoryMock);

        Func<Task> act = async () =>
            await service.DeleteAsync(targetEmployee.Id, currentEmployee.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("You cannot delete a user with equal or higher permissions.");
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldDeleteEmployee_WhenUserHasHigherRole()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var currentEmployee = CreateEmployee(EmployeeRoleEnum.Director);
        var targetEmployee = CreateEmployee(EmployeeRoleEnum.Leader);

        repositoryMock
            .Setup(r => r.GetByIdAsync(currentEmployee.Id))
            .ReturnsAsync(currentEmployee);

        repositoryMock
            .Setup(r => r.GetByIdAsync(targetEmployee.Id))
            .ReturnsAsync(targetEmployee);

        repositoryMock
            .Setup(r => r.DeleteAsync(targetEmployee))
            .Returns(Task.CompletedTask);

        var service = CreateService(repositoryMock);

        await service.DeleteAsync(targetEmployee.Id, currentEmployee.Id);

        repositoryMock.Verify(
            r => r.DeleteAsync(targetEmployee),
            Times.Once
        );
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldDeleteEmployee_WhenLeaderDeletesEmployee()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var currentEmployee = CreateEmployee(EmployeeRoleEnum.Leader);
        var targetEmployee = CreateEmployee(EmployeeRoleEnum.Employee);

        repositoryMock
            .Setup(r => r.GetByIdAsync(currentEmployee.Id))
            .ReturnsAsync(currentEmployee);

        repositoryMock
            .Setup(r => r.GetByIdAsync(targetEmployee.Id))
            .ReturnsAsync(targetEmployee);

        repositoryMock
            .Setup(r => r.DeleteAsync(targetEmployee))
            .Returns(Task.CompletedTask);

        var service = CreateService(repositoryMock);

        await service.DeleteAsync(targetEmployee.Id, currentEmployee.Id);

        repositoryMock.Verify(
            r => r.DeleteAsync(targetEmployee),
            Times.Once
        );
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldDeleteDirector_WhenDeletedByAnotherDirector()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var currentDirector = CreateEmployee(EmployeeRoleEnum.Director);
        var targetDirector = CreateEmployee(EmployeeRoleEnum.Director);

        repositoryMock
            .Setup(r => r.GetByIdAsync(currentDirector.Id))
            .ReturnsAsync(currentDirector);

        repositoryMock
            .Setup(r => r.GetByIdAsync(targetDirector.Id))
            .ReturnsAsync(targetDirector);

        repositoryMock
            .Setup(r => r.DeleteAsync(targetDirector))
            .Returns(Task.CompletedTask);

        var service = CreateService(repositoryMock);

        await service.DeleteAsync(targetDirector.Id, currentDirector.Id);

        repositoryMock.Verify(
            r => r.DeleteAsync(targetDirector),
            Times.Once
        );
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenDirectorDeletesHimself()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();
        var service = CreateService(repositoryMock);

        var directorId = Guid.NewGuid();

        Func<Task> act = async () =>
            await service.DeleteAsync(directorId, directorId);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("You cannot delete yourself.");
    }
    
    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenEmployeeHasSubordinates()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var director = CreateEmployee(EmployeeRoleEnum.Director);
        var manager = CreateEmployee(EmployeeRoleEnum.Leader);

        repositoryMock
            .Setup(r => r.GetByIdAsync(director.Id))
            .ReturnsAsync(director);

        repositoryMock
            .Setup(r => r.GetByIdAsync(manager.Id))
            .ReturnsAsync(manager);

        repositoryMock
            .Setup(r => r.HasSubordinatesAsync(manager.Id))
            .ReturnsAsync(true);

        var service = CreateService(repositoryMock);

        Func<Task> act = async () =>
            await service.DeleteAsync(manager.Id, director.Id);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("You cannot delete an employee who is a manager of other employees.");
    }
}