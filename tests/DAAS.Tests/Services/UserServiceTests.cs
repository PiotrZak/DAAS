using DAAS.Application.Services;
using DAAS.Domain.Entities;
using DAAS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DAAS.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _service = new UserService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Name = "User 1", Email = "user1@example.com", Role = UserRole.User },
            new() { Id = 2, Name = "Approver 1", Email = "approver1@example.com", Role = UserRole.Approver },
            new() { Id = 3, Name = "Admin 1", Email = "admin1@example.com", Role = UserRole.Admin }
        };

        _userRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(u => !string.IsNullOrEmpty(u.Name) && !string.IsNullOrEmpty(u.Email));
        
        var usersList = result.ToList();
        usersList[0].Name.Should().Be("User 1");
        usersList[0].Email.Should().Be("user1@example.com");
        usersList[0].Role.Should().Be(UserRole.User);
        
        _userRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            Role = UserRole.User
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Name.Should().Be("Test User");
        result.Email.Should().Be("test@example.com");
        result.Role.Should().Be(UserRole.User);

        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var userId = 999;
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ReturnsUser()
    {
        // Arrange
        var email = "test@example.com";
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = email,
            Role = UserRole.User
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test User");
        result.Email.Should().Be(email);
        result.Role.Should().Be(UserRole.User);

        _userRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithInvalidEmail_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        result.Should().BeNull();
        _userRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
    }

    [Theory]
    [InlineData(UserRole.User)]
    [InlineData(UserRole.Approver)]
    [InlineData(UserRole.Admin)]
    public async Task GetUserByIdAsync_WithDifferentRoles_ReturnsCorrectRole(UserRole role)
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            Role = role
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Role.Should().Be(role);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }
}