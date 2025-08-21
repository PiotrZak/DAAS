using DAAS.Application.DTOs;
using MediatR;

namespace DAAS.Application.Queries;

public record GetAllAccessRequestsQuery : IRequest<IEnumerable<AccessRequestDto>>;

public record GetUserAccessRequestsQuery(int UserId) : IRequest<IEnumerable<AccessRequestDto>>;

public record GetPendingAccessRequestsQuery : IRequest<IEnumerable<AccessRequestDto>>;

public record GetAllUsersQuery : IRequest<IEnumerable<UserDto>>;

public record GetUserByIdQuery(int Id) : IRequest<UserDto?>;

public record GetAllDocumentsQuery : IRequest<IEnumerable<DocumentDto>>;

public record GetDocumentByIdQuery(int Id) : IRequest<DocumentDto?>;