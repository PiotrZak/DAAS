using DAAS.Domain.Entities;

namespace DAAS.Application.DTOs;

public record CreateAccessRequestDto(
    int DocumentId,
    string Reason,
    AccessType AccessType
);

public record AccessRequestDto(
    int Id,
    int UserId,
    string UserName,
    int DocumentId,
    string DocumentTitle,
    string Reason,
    AccessType AccessType,
    DateTime RequestedAt,
    RequestStatus Status,
    DecisionDto? Decision
);

public record UserDto(
    int Id,
    string Name,
    string Email,
    UserRole Role
);

public record DocumentDto(
    int Id,
    string Title,
    string Description,
    DateTime CreatedAt
);

public record DecisionDto(
    int Id,
    int ApproverId,
    string ApproverName,
    bool IsApproved,
    string Comment,
    DateTime DecidedAt
);

public record CreateDecisionDto(
    bool IsApproved,
    string Comment
);