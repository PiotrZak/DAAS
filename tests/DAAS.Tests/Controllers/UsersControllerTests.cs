using DAAS.Api.Controllers;
using DAAS.Application.DTOs;
using DAAS.Application.Queries;
using DAAS.Domain.Entities;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DAAS.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UsersController(_mediatorMock.Object);
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

        _mediatorMock.Setup(x => x.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDtos);

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
        returnedUsers.Should().HaveCount(2);
        
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUsers_WhenMediatorThrows_ReturnsInternalServerError()
    {
        // Arrange
        _mediatorMock.Setup(x => x.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUsers();

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
        
        _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsOkWithUser()
    {
        // Arrange
        var userId = 1;
        var userDto = new UserDto(userId, "Test User", "test@example.com", UserRole.User);

        _mediatorMock.Setup(x => x.Send(It.Is<GetUserByIdQuery>(q => q.Id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUser = okResult.Value.Should().BeOfType<UserDto>().Subject;
        returnedUser.Id.Should().Be(userId);
        returnedUser.Name.Should().Be("Test User");
        
        _mediatorMock.Verify(x => x.Send(It.Is<GetUserByIdQuery>(q => q.Id == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;
        _mediatorMock.Setup(x => x.Send(It.Is<GetUserByIdQuery>(q => q.Id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().BeEquivalentTo(new { message = "User not found" });
        
        _mediatorMock.Verify(x => x.Send(It.Is<GetUserByIdQuery>(q => q.Id == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUser_WhenMediatorThrows_ReturnsInternalServerError()
    {
        // Arrange
        var userId = 1;
        _mediatorMock.Setup(x => x.Send(It.Is<GetUserByIdQuery>(q => q.Id == userId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
        
        _mediatorMock.Verify(x => x.Send(It.Is<GetUserByIdQuery>(q => q.Id == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1, "User 1")]
    [InlineData(2, "Admin User")]
    [InlineData(100, "Test User")]
    public async Task GetUser_WithVariousValidIds_ReturnsCorrectUser(int userId, string userName)
    {
        // Arrange
        var userDto = new UserDto(userId, userName, "test@example.com", UserRole.User);
        _mediatorMock.Setup(x => x.Send(It.Is<GetUserByIdQuery>(q => q.Id == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedUser = okResult.Value.Should().BeOfType<UserDto>().Subject;
        returnedUser.Id.Should().Be(userId);
        returnedUser.Name.Should().Be(userName);
        
        _mediatorMock.Verify(x => x.Send(It.Is<GetUserByIdQuery>(q => q.Id == userId), It.IsAny<CancellationToken>()), Times.Once);
    }
}