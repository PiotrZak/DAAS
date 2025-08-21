using DAAS.Application.DTOs;
using DAAS.Domain.Entities;
using MediatR;

namespace DAAS.Application.Commands;

public record CreateAccessRequestCommand(
    int UserId,
    int DocumentId,
    string Reason,
    AccessType AccessType
) : IRequest<AccessRequestDto>;

public record ApproveAccessRequestCommand(
    int RequestId,
    int ApproverId,
    bool IsApproved,
    string Comment
) : IRequest<AccessRequestDto>;