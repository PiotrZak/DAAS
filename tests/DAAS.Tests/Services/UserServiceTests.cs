using DAAS.Application.DTOs;
using DAAS.Application.Handlers;
using DAAS.Application.Queries;
using DAAS.Domain.Entities;
using DAAS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DAAS.Tests.Services;

public class UserQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;

    public UserQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
    }

    [Fact]
    public async Task GetAllUsersQueryHandler_ReturnsAllUsers()
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

        var handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object);
        var query = new GetAllUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

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
    public async Task GetUserByIdQueryHandler_WithValidId_ReturnsUser()
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

        var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Name.Should().Be("Test User");
        result.Email.Should().Be("test@example.com");
        result.Role.Should().Be(UserRole.User);

        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdQueryHandler_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var userId = 999;
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersQueryHandler_WithEmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<User>());

        var handler = new GetAllUsersQueryHandler(_userRepositoryMock.Object);
        var query = new GetAllUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _userRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Theory]
    [InlineData(UserRole.User)]
    [InlineData(UserRole.Approver)]
    [InlineData(UserRole.Admin)]
    public async Task GetUserByIdQueryHandler_WithDifferentRoles_ReturnsCorrectRole(UserRole role)
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

        var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Role.Should().Be(role);
        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdQueryHandler_RepositoryMapping_MapsAllProperties()
    {
        // Arrange
        var userId = 5;
        var user = new User
        {
            Id = userId,
            Name = "Complex User Name",
            Email = "complex.user@example.com",
            Role = UserRole.Approver
        };

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Name.Should().Be("Complex User Name");
        result.Email.Should().Be("complex.user@example.com");
        result.Role.Should().Be(UserRole.Approver);

        _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }
}