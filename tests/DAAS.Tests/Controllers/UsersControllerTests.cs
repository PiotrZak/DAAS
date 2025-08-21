using DAAS.Api.Controllers;
using DAAS.Application.DTOs;
using DAAS.Application.Services;
using DAAS.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DAAS.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _controller = new UsersController(_userServiceMock.Object);
    }

    [Fact]
    public async Task GetUsers_ReturnsOkWithUsers()
    {
        // Arrange
        var userDtos = new List<UserDto>
        {
            new(1, "User 1", "user1@example.com", UserRole.User),
            new(2, "Approver 1", "approver1@example.com", UserRole.Approver)
        };

        _userServiceMock.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(userDtos);

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
        returnedUsers.Should().HaveCount(2);
        
        _userServiceMock.Verify(x => x.GetAllUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUsers_WhenServiceThrows_ReturnsInternalServerError()
    {
        // Arrange
        _userServiceMock.Setup(x => x.GetAllUsersAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
        
        _userServiceMock.Verify(x => x.GetAllUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsOkWithUser()
    {
        // Arrange
        var userId = 1;
        var userDto = new UserDto(userId, "Test User", "test@example.com", UserRole.User);

        _userServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUser = okResult.Value.Should().BeOfType<UserDto>().Subject;
        returnedUser.Id.Should().Be(userId);
        returnedUser.Name.Should().Be("Test User");
        
        _userServiceMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;
        _userServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().BeEquivalentTo(new { message = "User not found" });
        
        _userServiceMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUser_WhenServiceThrows_ReturnsInternalServerError()
    {
        // Arrange
        var userId = 1;
        _userServiceMock.Setup(x => x.GetUserByIdAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
        
        _userServiceMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmail_WithValidEmail_ReturnsOkWithUser()
    {
        // Arrange
        var email = "test@example.com";
        var userDto = new UserDto(1, "Test User", email, UserRole.User);

        _userServiceMock.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetUserByEmail(email);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUser = okResult.Value.Should().BeOfType<UserDto>().Subject;
        returnedUser.Email.Should().Be(email);
        returnedUser.Name.Should().Be("Test User");
        
        _userServiceMock.Verify(x => x.GetUserByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmail_WithInvalidEmail_ReturnsNotFound()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _userServiceMock.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetUserByEmail(email);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().BeEquivalentTo(new { message = "User not found" });
        
        _userServiceMock.Verify(x => x.GetUserByEmailAsync(email), Times.Once);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("admin@company.com")]
    [InlineData("test.user+tag@domain.co.uk")]
    public async Task GetUserByEmail_WithVariousValidEmails_ReturnsCorrectUser(string email)
    {
        // Arrange
        var userDto = new UserDto(1, "Test User", email, UserRole.User);
        _userServiceMock.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetUserByEmail(email);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUser = okResult.Value.Should().BeOfType<UserDto>().Subject;
        returnedUser.Email.Should().Be(email);
        
        _userServiceMock.Verify(x => x.GetUserByEmailAsync(email), Times.Once);
    }
}