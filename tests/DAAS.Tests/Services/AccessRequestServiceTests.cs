using DAAS.Application.DTOs;
using DAAS.Application.Services;
using DAAS.Domain.Entities;
using DAAS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DAAS.Tests.Services;

public class AccessRequestServiceTests
{
    private readonly Mock<IAccessRequestRepository> _accessRequestRepositoryMock;
    private readonly Mock<IDecisionRepository> _decisionRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly AccessRequestService _service;

    public AccessRequestServiceTests()
    {
        _accessRequestRepositoryMock = new Mock<IAccessRequestRepository>();
        _decisionRepositoryMock = new Mock<IDecisionRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        
        _service = new AccessRequestService(
            _accessRequestRepositoryMock.Object,
            _decisionRepositoryMock.Object,
            _userRepositoryMock.Object,
            _documentRepositoryMock.Object
        );
    }

    [Fact]
    public async Task CreateAccessRequestAsync_WithValidData_CreatesAccessRequest()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Role = UserRole.User };
        var documentId = 1;
        var document = new Document { Id = documentId, Title = "Test Document", Description = "Test Description", CreatedAt = DateTime.UtcNow };
        var createDto = new CreateAccessRequestDto(documentId, "Need access for testing", AccessType.Read);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
            .ReturnsAsync(document);

        var expectedAccessRequest = new AccessRequest
        {
            Id = 1,
            UserId = userId,
            DocumentId = documentId,
            Reason = createDto.Reason,
            AccessType = createDto.AccessType,
            Status = RequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            User = user,
            Document = document
        };

        _accessRequestRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<AccessRequest>()))
            .ReturnsAsync(expectedAccessRequest);

        // Act
        var result = await _service.CreateAccessRequestAsync(userId, createDto);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.DocumentId.Should().Be(documentId);
        result.Reason.Should().Be(createDto.Reason);
        result.AccessType.Should().Be(createDto.AccessType);
        result.Status.Should().Be(RequestStatus.Pending);
        result.UserName.Should().Be(user.Name);
        result.DocumentTitle.Should().Be(document.Title);

        _accessRequestRepositoryMock.Verify(x => x.CreateAsync(It.Is<AccessRequest>(ar => 
            ar.UserId == userId &&
            ar.DocumentId == documentId &&
            ar.Reason == createDto.Reason &&
            ar.AccessType == createDto.AccessType &&
            ar.Status == RequestStatus.Pending)), Times.Once);
    }

    [Fact]
    public async Task CreateAccessRequestAsync_WithInvalidUser_ThrowsArgumentException()
    {
        // Arrange
        var userId = 999;
        var createDto = new CreateAccessRequestDto(1, "Need access", AccessType.Read);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await FluentActions.Invoking(() => _service.CreateAccessRequestAsync(userId, createDto))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("User not found*");

        _accessRequestRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<AccessRequest>()), Times.Never);
    }

    [Fact]
    public async Task MakeDecisionAsync_WithValidApproval_ApprovesRequest()
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

        var decisionDto = new CreateDecisionDto(true, "Access granted for testing");

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
            Comment = decisionDto.Comment,
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
            Status = RequestStatus.Approved, // Updated status
            RequestedAt = DateTime.UtcNow,
            User = new User { Id = 1, Name = "User", Email = "user@example.com", Role = UserRole.User },
            Document = new Document { Id = 1, Title = "Document", Description = "Test doc", CreatedAt = DateTime.UtcNow },
            Decision = expectedDecision
        };

        _accessRequestRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<AccessRequest>()))
            .ReturnsAsync(updatedAccessRequest);

        // Act
        var result = await _service.MakeDecisionAsync(requestId, approverId, decisionDto);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(RequestStatus.Approved);
        result.Decision.Should().NotBeNull();
        result.Decision!.IsApproved.Should().BeTrue();
        result.Decision.Comment.Should().Be(decisionDto.Comment);
        result.Decision.ApproverName.Should().Be(approver.Name);

        _decisionRepositoryMock.Verify(x => x.CreateAsync(It.Is<Decision>(d => 
            d.AccessRequestId == requestId &&
            d.ApproverId == approverId &&
            d.IsApproved == true &&
            d.Comment == decisionDto.Comment)), Times.Once);

        _accessRequestRepositoryMock.Verify(x => x.UpdateAsync(It.Is<AccessRequest>(ar => 
            ar.Id == requestId && ar.Status == RequestStatus.Approved)), Times.Once);
    }

    [Fact]
    public async Task MakeDecisionAsync_WithNonApprover_ThrowsUnauthorizedAccessException()
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

        var decisionDto = new CreateDecisionDto(true, "Trying to approve");

        _accessRequestRepositoryMock.Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(accessRequest);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        await FluentActions.Invoking(() => _service.MakeDecisionAsync(requestId, userId, decisionDto))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is not authorized to approve requests");

        _decisionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Decision>()), Times.Never);
        _accessRequestRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<AccessRequest>()), Times.Never);
    }

    [Fact]
    public async Task MakeDecisionAsync_WithAlreadyDecidedRequest_ThrowsInvalidOperationException()
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

        var decisionDto = new CreateDecisionDto(false, "Trying to change decision");

        _accessRequestRepositoryMock.Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(accessRequest);

        _userRepositoryMock.Setup(x => x.GetByIdAsync(approverId))
            .ReturnsAsync(approver);

        // Act & Assert
        await FluentActions.Invoking(() => _service.MakeDecisionAsync(requestId, approverId, decisionDto))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Request has already been decided");

        _decisionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Decision>()), Times.Never);
        _accessRequestRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<AccessRequest>()), Times.Never);
    }

    [Fact]
    public async Task GetPendingRequestsAsync_ReturnsPendingRequestsOnly()
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

        // Act
        var result = await _service.GetPendingRequestsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Status == RequestStatus.Pending);
        
        _accessRequestRepositoryMock.Verify(x => x.GetPendingRequestsAsync(), Times.Once);
    }
}