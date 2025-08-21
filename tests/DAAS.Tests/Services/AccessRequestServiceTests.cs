using DAAS.Application.Commands;
using DAAS.Application.DTOs;
using DAAS.Application.Handlers;
using DAAS.Application.Queries;
using DAAS.Domain.Entities;
using DAAS.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace DAAS.Tests.Services;

public class AccessRequestCommandHandlerTests
{
    private readonly Mock<IAccessRequestRepository> _accessRequestRepositoryMock;
    private readonly Mock<IDecisionRepository> _decisionRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<IMediator> _mediatorMock;

    public AccessRequestCommandHandlerTests()
    {
        _accessRequestRepositoryMock = new Mock<IAccessRequestRepository>();
        _decisionRepositoryMock = new Mock<IDecisionRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _mediatorMock = new Mock<IMediator>();
    }

    [Fact]
    public async Task CreateAccessRequestCommandHandler_WithValidData_CreatesAccessRequest()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Role = UserRole.User };
        var documentId = 1;
        var document = new Document { Id = documentId, Title = "Test Document", Description = "Test Description", CreatedAt = DateTime.UtcNow };
        var command = new CreateAccessRequestCommand(userId, documentId, "Need access for testing", AccessType.Read);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
            .ReturnsAsync(document);

        var expectedAccessRequest = new AccessRequest
        {
            Id = 1,
            UserId = userId,
            DocumentId = documentId,
            Reason = command.Reason,
            AccessType = command.AccessType,
            Status = RequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            User = user,
            Document = document
        };

        _accessRequestRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<AccessRequest>()))
            .ReturnsAsync(expectedAccessRequest);

        var handler = new CreateAccessRequestCommandHandler(
            _accessRequestRepositoryMock.Object,
            _userRepositoryMock.Object,
            _documentRepositoryMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.DocumentId.Should().Be(documentId);
        result.Reason.Should().Be(command.Reason);
        result.AccessType.Should().Be(command.AccessType);
        result.Status.Should().Be(RequestStatus.Pending);
        result.UserName.Should().Be(user.Name);
        result.DocumentTitle.Should().Be(document.Title);

        _accessRequestRepositoryMock.Verify(x => x.CreateAsync(It.Is<AccessRequest>(ar => 
            ar.UserId == userId &&
            ar.DocumentId == documentId &&
            ar.Reason == command.Reason &&
            ar.AccessType == command.AccessType &&
            ar.Status == RequestStatus.Pending)), Times.Once);
    }

    [Fact]
    public async Task CreateAccessRequestCommandHandler_WithInvalidUser_ThrowsArgumentException()
    {
        // Arrange
        var userId = 999;
        var command = new CreateAccessRequestCommand(userId, 1, "Need access", AccessType.Read);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var handler = new CreateAccessRequestCommandHandler(
            _accessRequestRepositoryMock.Object,
            _userRepositoryMock.Object,
            _documentRepositoryMock.Object);

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("User not found*");

        _accessRequestRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<AccessRequest>()), Times.Never);
    }

    [Fact]
    public async Task CreateAccessRequestCommandHandler_WithInvalidDocument_ThrowsArgumentException()
    {
        // Arrange
        var userId = 1;
        var documentId = 999;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Role = UserRole.User };
        var command = new CreateAccessRequestCommand(userId, documentId, "Need access", AccessType.Read);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
            .ReturnsAsync((Document?)null);

        var handler = new CreateAccessRequestCommandHandler(
            _accessRequestRepositoryMock.Object,
            _userRepositoryMock.Object,
            _documentRepositoryMock.Object);

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Document not found*");

        _accessRequestRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<AccessRequest>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAccessRequestCommandHandler_WithValidApproval_ApprovesRequest()
    {
        // Arrange
        var requestId = 1;
        var approverId = 2;
        var approver = new User { Id = approverId, Name = "Approver", Email = "approver@example.com", Role = UserRole.Approver };
        var accessRequest = new AccessRequest
        {
            Id = requestId,
            UserId = 1,
            DocumentId = 1,
            Reason = "Test reason",
            AccessType = AccessType.Read,
            Status = RequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            User = new User { Id = 1, Name = "User", Email = "user@example.com", Role = UserRole.User },
            Document = new Document { Id = 1, Title = "Document", Description = "Test doc", CreatedAt = DateTime.UtcNow }
        };

        var command = new ApproveAccessRequestCommand(requestId, approverId, true, "Access granted for testing");

        _accessRequestRepositoryMock.Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(accessRequest);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(approverId))
            .ReturnsAsync(approver);

        var expectedDecision = new Decision
        {
            Id = 1,
            AccessRequestId = requestId,
            ApproverId = approverId,
            IsApproved = true,
            Comment = command.Comment,
            DecidedAt = DateTime.UtcNow,
            Approver = approver
        };

        _decisionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Decision>()))
            .ReturnsAsync(expectedDecision);

        var updatedAccessRequest = new AccessRequest
        {
            Id = requestId,
            UserId = 1,
            DocumentId = 1,
            Reason = "Test reason",
            AccessType = AccessType.Read,
            Status = RequestStatus.Approved,
            RequestedAt = DateTime.UtcNow,
            User = new User { Id = 1, Name = "User", Email = "user@example.com", Role = UserRole.User },
            Document = new Document { Id = 1, Title = "Document", Description = "Test doc", CreatedAt = DateTime.UtcNow },
            Decision = expectedDecision
        };

        _accessRequestRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<AccessRequest>()))
            .ReturnsAsync(updatedAccessRequest);

        var handler = new ApproveAccessRequestCommandHandler(
            _accessRequestRepositoryMock.Object,
            _decisionRepositoryMock.Object,
            _userRepositoryMock.Object,
            _mediatorMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(RequestStatus.Approved);
        result.Decision.Should().NotBeNull();
        result.Decision!.IsApproved.Should().BeTrue();
        result.Decision.Comment.Should().Be(command.Comment);
        result.Decision.ApproverName.Should().Be(approver.Name);

        _decisionRepositoryMock.Verify(x => x.CreateAsync(It.Is<Decision>(d => 
            d.AccessRequestId == requestId &&
            d.ApproverId == approverId &&
            d.IsApproved == true &&
            d.Comment == command.Comment)), Times.Once);

        _accessRequestRepositoryMock.Verify(x => x.UpdateAsync(It.Is<AccessRequest>(ar => 
            ar.Id == requestId && ar.Status == RequestStatus.Approved)), Times.Once);

        _mediatorMock.Verify(x => x.Publish(It.IsAny<AccessRequestDecisionMadeEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveAccessRequestCommandHandler_WithNonApprover_ThrowsInvalidOperationException()
    {
        // Arrange
        var requestId = 1;
        var userId = 2;
        var user = new User { Id = userId, Name = "Regular User", Email = "user@example.com", Role = UserRole.User };
        var accessRequest = new AccessRequest
        {
            Id = requestId,
            Status = RequestStatus.Pending
        };

        var command = new ApproveAccessRequestCommand(requestId, userId, true, "Trying to approve");

        _accessRequestRepositoryMock.Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(accessRequest);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var handler = new ApproveAccessRequestCommandHandler(
            _accessRequestRepositoryMock.Object,
            _decisionRepositoryMock.Object,
            _userRepositoryMock.Object,
            _mediatorMock.Object);

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User is not authorized to approve requests");

        _decisionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Decision>()), Times.Never);
        _accessRequestRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<AccessRequest>()), Times.Never);
    }

    [Fact]
    public async Task ApproveAccessRequestCommandHandler_WithAlreadyDecidedRequest_ThrowsInvalidOperationException()
    {
        // Arrange
        var requestId = 1;
        var approverId = 2;
        var approver = new User { Id = approverId, Name = "Approver", Email = "approver@example.com", Role = UserRole.Approver };
        var accessRequest = new AccessRequest
        {
            Id = requestId,
            Status = RequestStatus.Approved // Already decided
        };

        var command = new ApproveAccessRequestCommand(requestId, approverId, false, "Trying to change decision");

        _accessRequestRepositoryMock.Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(accessRequest);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(approverId))
            .ReturnsAsync(approver);

        var handler = new ApproveAccessRequestCommandHandler(
            _accessRequestRepositoryMock.Object,
            _decisionRepositoryMock.Object,
            _userRepositoryMock.Object,
            _mediatorMock.Object);

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Access request is not in pending status");

        _decisionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Decision>()), Times.Never);
        _accessRequestRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<AccessRequest>()), Times.Never);
    }
}

public class AccessRequestQueryHandlerTests
{
    private readonly Mock<IAccessRequestRepository> _accessRequestRepositoryMock;

    public AccessRequestQueryHandlerTests()
    {
        _accessRequestRepositoryMock = new Mock<IAccessRequestRepository>();
    }

    [Fact]
    public async Task GetPendingAccessRequestsQueryHandler_ReturnsPendingRequestsOnly()
    {
        // Arrange
        var pendingRequests = new List<AccessRequest>
        {
            new()
            {
                Id = 1,
                UserId = 1,
                DocumentId = 1,
                Status = RequestStatus.Pending,
                User = new User { Name = "User1" },
                Document = new Document { Title = "Doc1" }
            },
            new()
            {
                Id = 2,
                UserId = 2,
                DocumentId = 2,
                Status = RequestStatus.Pending,
                User = new User { Name = "User2" },
                Document = new Document { Title = "Doc2" }
            }
        };

        _accessRequestRepositoryMock.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(pendingRequests);

        var handler = new GetPendingAccessRequestsQueryHandler(_accessRequestRepositoryMock.Object);
        var query = new GetPendingAccessRequestsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Status == RequestStatus.Pending);
        
        _accessRequestRepositoryMock.Verify(x => x.GetPendingRequestsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAccessRequestsQueryHandler_ReturnsAllRequests()
    {
        // Arrange
        var requests = new List<AccessRequest>
        {
            new()
            {
                Id = 1,
                UserId = 1,
                DocumentId = 1,
                Status = RequestStatus.Pending,
                User = new User { Name = "User1" },
                Document = new Document { Title = "Doc1" }
            },
            new()
            {
                Id = 2,
                UserId = 2,
                DocumentId = 2,
                Status = RequestStatus.Approved,
                User = new User { Name = "User2" },
                Document = new Document { Title = "Doc2" }
            }
        };

        _accessRequestRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(requests);

        var handler = new GetAllAccessRequestsQueryHandler(_accessRequestRepositoryMock.Object);
        var query = new GetAllAccessRequestsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        
        _accessRequestRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUserAccessRequestsQueryHandler_ReturnsUserRequests()
    {
        // Arrange
        var userId = 1;
        var userRequests = new List<AccessRequest>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                DocumentId = 1,
                Status = RequestStatus.Pending,
                User = new User { Name = "User1" },
                Document = new Document { Title = "Doc1" }
            }
        };

        _accessRequestRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(userRequests);

        var handler = new GetUserAccessRequestsQueryHandler(_accessRequestRepositoryMock.Object);
        var query = new GetUserAccessRequestsQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(userId);
        
        _accessRequestRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
    }
}