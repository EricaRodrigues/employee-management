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
    public async Task CreateAsync_ShouldThrowException_WhenDocumentAlreadyExists()
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
            .WithMessage("You cannot create a user with higher permissions than yours.");
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
}