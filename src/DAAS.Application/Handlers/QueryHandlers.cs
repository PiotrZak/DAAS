using DAAS.Application.DTOs;
using DAAS.Application.Queries;
using DAAS.Domain.Interfaces;
using MediatR;

namespace DAAS.Application.Handlers;

public class GetAllAccessRequestsQueryHandler(IAccessRequestRepository accessRequestRepository) 
    : IRequestHandler<GetAllAccessRequestsQuery, IEnumerable<AccessRequestDto>>
{
    private readonly IAccessRequestRepository _accessRequestRepository = accessRequestRepository;

    public async Task<IEnumerable<AccessRequestDto>> Handle(GetAllAccessRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _accessRequestRepository.GetAllAsync();
        return requests.Select(MapToDto);
    }

    private static AccessRequestDto MapToDto(DAAS.Domain.Entities.AccessRequest request)
    {
        return new AccessRequestDto(
            request.Id,
            request.UserId,
            request.User?.Name ?? "Unknown",
            request.DocumentId,
            request.Document?.Title ?? "Unknown",
            request.Reason,
            request.AccessType,
            request.RequestedAt,
            request.Status,
            request.Decision != null ? new DecisionDto(
                request.Decision.Id,
                request.Decision.ApproverId,
                request.Decision.Approver?.Name ?? "Unknown",
                request.Decision.IsApproved,
                request.Decision.Comment,
                request.Decision.DecidedAt
            ) : null
        );
    }
}

public class GetUserAccessRequestsQueryHandler(IAccessRequestRepository accessRequestRepository) 
    : IRequestHandler<GetUserAccessRequestsQuery, IEnumerable<AccessRequestDto>>
{
    private readonly IAccessRequestRepository _accessRequestRepository = accessRequestRepository;

    public async Task<IEnumerable<AccessRequestDto>> Handle(GetUserAccessRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _accessRequestRepository.GetByUserIdAsync(request.UserId);
        return requests.Select(MapToDto);
    }

    private static AccessRequestDto MapToDto(DAAS.Domain.Entities.AccessRequest request)
    {
        return new AccessRequestDto(
            request.Id,
            request.UserId,
            request.User?.Name ?? "Unknown",
            request.DocumentId,
            request.Document?.Title ?? "Unknown",
            request.Reason,
            request.AccessType,
            request.RequestedAt,
            request.Status,
            request.Decision != null ? new DecisionDto(
                request.Decision.Id,
                request.Decision.ApproverId,
                request.Decision.Approver?.Name ?? "Unknown",
                request.Decision.IsApproved,
                request.Decision.Comment,
                request.Decision.DecidedAt
            ) : null
        );
    }
}

public class GetPendingAccessRequestsQueryHandler(IAccessRequestRepository accessRequestRepository) 
    : IRequestHandler<GetPendingAccessRequestsQuery, IEnumerable<AccessRequestDto>>
{
    private readonly IAccessRequestRepository _accessRequestRepository = accessRequestRepository;

    public async Task<IEnumerable<AccessRequestDto>> Handle(GetPendingAccessRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _accessRequestRepository.GetPendingRequestsAsync();
        return requests.Select(MapToDto);
    }

    private static AccessRequestDto MapToDto(DAAS.Domain.Entities.AccessRequest request)
    {
        return new AccessRequestDto(
            request.Id,
            request.UserId,
            request.User?.Name ?? "Unknown",
            request.DocumentId,
            request.Document?.Title ?? "Unknown",
            request.Reason,
            request.AccessType,
            request.RequestedAt,
            request.Status,
            request.Decision != null ? new DecisionDto(
                request.Decision.Id,
                request.Decision.ApproverId,
                request.Decision.Approver?.Name ?? "Unknown",
                request.Decision.IsApproved,
                request.Decision.Comment,
                request.Decision.DecidedAt
            ) : null
        );
    }
}

public class GetAllUsersQueryHandler(IUserRepository userRepository) 
    : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(user => new UserDto(user.Id, user.Name, user.Email, user.Role));
    }
}

public class GetUserByIdQueryHandler(IUserRepository userRepository) 
    : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id);
        return user != null ? new UserDto(user.Id, user.Name, user.Email, user.Role) : null;
    }
}

public class GetAllDocumentsQueryHandler(IDocumentRepository documentRepository) 
    : IRequestHandler<GetAllDocumentsQuery, IEnumerable<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository = documentRepository;

    public async Task<IEnumerable<DocumentDto>> Handle(GetAllDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetAllAsync();
        return documents.Select(doc => new DocumentDto(doc.Id, doc.Title, doc.Description, doc.CreatedAt));
    }
}

public class GetDocumentByIdQueryHandler(IDocumentRepository documentRepository) 
    : IRequestHandler<GetDocumentByIdQuery, DocumentDto?>
{
    private readonly IDocumentRepository _documentRepository = documentRepository;

    public async Task<DocumentDto?> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.Id);
        return document != null ? new DocumentDto(document.Id, document.Title, document.Description, document.CreatedAt) : null;
    }
}