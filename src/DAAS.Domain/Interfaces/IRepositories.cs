using DAAS.Domain.Entities;

namespace DAAS.Domain.Interfaces;

public interface IAccessRequestRepository
{
    Task<IEnumerable<AccessRequest>> GetAllAsync();
    Task<IEnumerable<AccessRequest>> GetByUserIdAsync(int userId);
    Task<IEnumerable<AccessRequest>> GetPendingRequestsAsync();
    Task<AccessRequest?> GetByIdAsync(int id);
    Task<AccessRequest> CreateAsync(AccessRequest accessRequest);
    Task<AccessRequest> UpdateAsync(AccessRequest accessRequest);
}

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
}

public interface IDocumentRepository
{
    Task<IEnumerable<Document>> GetAllAsync();
    Task<Document?> GetByIdAsync(int id);
    Task<Document> CreateAsync(Document document);
}

public interface IDecisionRepository
{
    Task<Decision> CreateAsync(Decision decision);
    Task<Decision?> GetByAccessRequestIdAsync(int accessRequestId);
}