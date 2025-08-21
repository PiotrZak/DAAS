using DAAS.Application.Commands;
using DAAS.Application.DTOs;
using DAAS.Domain.Entities;
using DAAS.Domain.Interfaces;
using MediatR;

namespace DAAS.Application.Handlers;

public class CreateAccessRequestCommandHandler(
    IAccessRequestRepository accessRequestRepository,
    IUserRepository userRepository,
    IDocumentRepository documentRepository) : IRequestHandler<CreateAccessRequestCommand, AccessRequestDto>
{
    private readonly IAccessRequestRepository _accessRequestRepository = accessRequestRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IDocumentRepository _documentRepository = documentRepository;

    public async Task<AccessRequestDto> Handle(CreateAccessRequestCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(request.UserId));

        var document = await _documentRepository.GetByIdAsync(request.DocumentId);
        if (document == null)
            throw new ArgumentException("Document not found", nameof(request.DocumentId));

        var accessRequest = new AccessRequest
        {
            UserId = request.UserId,
            DocumentId = request.DocumentId,
            Reason = request.Reason,
            AccessType = request.AccessType,
            RequestedAt = DateTime.UtcNow,
            Status = RequestStatus.Pending
        };

        var createdRequest = await _accessRequestRepository.CreateAsync(accessRequest);
        
        return new AccessRequestDto(
            createdRequest.Id,
            createdRequest.UserId,
            user.Name,
            createdRequest.DocumentId,
            document.Title,
            createdRequest.Reason,
            createdRequest.AccessType,
            createdRequest.RequestedAt,
            createdRequest.Status,
            null
        );
    }
}

public class ApproveAccessRequestCommandHandler(
    IAccessRequestRepository accessRequestRepository,
    IDecisionRepository decisionRepository,
    IUserRepository userRepository,
    IMediator mediator) : IRequestHandler<ApproveAccessRequestCommand, AccessRequestDto>
{
    private readonly IAccessRequestRepository _accessRequestRepository = accessRequestRepository;
    private readonly IDecisionRepository _decisionRepository = decisionRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IMediator _mediator = mediator;

    public async Task<AccessRequestDto> Handle(ApproveAccessRequestCommand request, CancellationToken cancellationToken)
    {
        var accessRequest = await _accessRequestRepository.GetByIdAsync(request.RequestId);
        if (accessRequest == null)
            throw new ArgumentException("Access request not found", nameof(request.RequestId));

        if (accessRequest.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Access request is not in pending status");

        var approver = await _userRepository.GetByIdAsync(request.ApproverId);
        if (approver == null)
            throw new ArgumentException("Approver not found", nameof(request.ApproverId));

        if (approver.Role != UserRole.Approver && approver.Role != UserRole.Admin)
            throw new InvalidOperationException("User is not authorized to approve requests");

        var decision = new Decision
        {
            AccessRequestId = request.RequestId,
            ApproverId = request.ApproverId,
            IsApproved = request.IsApproved,
            Comment = request.Comment,
            DecidedAt = DateTime.UtcNow
        };

        accessRequest.Status = request.IsApproved ? RequestStatus.Approved : RequestStatus.Rejected;
        
        await _decisionRepository.CreateAsync(decision);
        await _accessRequestRepository.UpdateAsync(accessRequest);

        // Trigger notification event
        await _mediator.Publish(new AccessRequestDecisionMadeEvent(
            accessRequest.Id,
            accessRequest.UserId,
            request.ApproverId,
            request.IsApproved,
            request.Comment
        ), cancellationToken);

        return new AccessRequestDto(
            accessRequest.Id,
            accessRequest.UserId,
            accessRequest.User?.Name ?? "Unknown",
            accessRequest.DocumentId,
            accessRequest.Document?.Title ?? "Unknown",
            accessRequest.Reason,
            accessRequest.AccessType,
            accessRequest.RequestedAt,
            accessRequest.Status,
            new DecisionDto(
                decision.Id,
                decision.ApproverId,
                approver.Name,
                decision.IsApproved,
                decision.Comment,
                decision.DecidedAt
            )
        );
    }
}

// Event for notification system
public record AccessRequestDecisionMadeEvent(
    int RequestId,
    int UserId,
    int ApproverId,
    bool IsApproved,
    string Comment
) : INotification;