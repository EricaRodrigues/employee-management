using EmployeeManagement.Application.DTOs;
using EmployeeManagement.Application.Exceptions;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Repositories.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EmployeeManagement.Tests.Services;

public class AuthServiceTests
{
    // ----------------------------
    // Helpers
    // ----------------------------
    private static IConfiguration CreateFakeConfiguration()
    {
        var settings = new Dictionary<string, string>
        {
            { "Jwt:Key", "THIS_IS_A_VERY_LONG_TEST_KEY_FOR_JWT_TOKEN" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();
    }
    
    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenEmailDoesNotExist()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        repositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Employee?)null);

        var configuration = CreateFakeConfiguration();

        var service = new AuthService(repositoryMock.Object, configuration);

        var request = new LoginRequestDTO(
            Email: "invalid@company.com",
            Password: "password123"
        );

        Func<Task> act = async () =>
            await service.LoginAsync(request);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Invalid credentials.");
    }
    
    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenPasswordIsInvalid()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var employee = new Employee(
            firstName: "Erica",
            lastName: "Rodrigues",
            email: "erica@company.com",
            docNumber: "123456789",
            birthDate: DateTime.UtcNow.AddYears(-30),
            role: EmployeeRoleEnum.Employee,
            passwordHash: BCrypt.Net.BCrypt.HashPassword("correct-password")
        );

        repositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(employee);

        var configuration = CreateFakeConfiguration();

        var service = new AuthService(repositoryMock.Object, configuration);

        var request = new LoginRequestDTO(
            Email: "erica@company.com",
            Password: "wrong-password"
        );

        Func<Task> act = async () =>
            await service.LoginAsync(request);

        await act
            .Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("Invalid credentials.");
    }
    
    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var repositoryMock = new Mock<IEmployeeRepository>();

        var password = "password123";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var employee = new Employee(
            firstName: "Erica",
            lastName: "Rodrigues",
            email: "erica@company.com",
            docNumber: "987654321",
            birthDate: DateTime.UtcNow.AddYears(-30),
            role: EmployeeRoleEnum.Employee,
            passwordHash: hashedPassword
        );

        repositoryMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(employee);

        var configuration = CreateFakeConfiguration();

        var service = new AuthService(repositoryMock.Object, configuration);

        var request = new LoginRequestDTO(
            Email: "erica@company.com",
            Password: password
        );

        var result = await service.LoginAsync(request);

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrWhiteSpace();
    }
}