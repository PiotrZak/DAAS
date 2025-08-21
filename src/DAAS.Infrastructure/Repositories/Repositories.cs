using DAAS.Domain.Entities;
using DAAS.Domain.Interfaces;
using DAAS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DAAS.Infrastructure.Repositories;

public class AccessRequestRepository(ApplicationDbContext context) : IAccessRequestRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<AccessRequest>> GetAllAsync()
    {
        return await _context.AccessRequests
            .Include(ar => ar.User)
            .Include(ar => ar.Document)
            .Include(ar => ar.Decision)
            .ThenInclude(d => d!.Approver)
            .ToListAsync();
    }

    public async Task<IEnumerable<AccessRequest>> GetByUserIdAsync(int userId)
    {
        return await _context.AccessRequests
            .Where(ar => ar.UserId == userId)
            .Include(ar => ar.User)
            .Include(ar => ar.Document)
            .Include(ar => ar.Decision)
            .ThenInclude(d => d!.Approver)
            .ToListAsync();
    }

    public async Task<IEnumerable<AccessRequest>> GetPendingRequestsAsync()
    {
        return await _context.AccessRequests
            .Where(ar => ar.Status == RequestStatus.Pending)
            .Include(ar => ar.User)
            .Include(ar => ar.Document)
            .ToListAsync();
    }

    public async Task<AccessRequest?> GetByIdAsync(int id)
    {
        return await _context.AccessRequests
            .Include(ar => ar.User)
            .Include(ar => ar.Document)
            .Include(ar => ar.Decision)
            .ThenInclude(d => d!.Approver)
            .FirstOrDefaultAsync(ar => ar.Id == id);
    }

    public async Task<AccessRequest> CreateAsync(AccessRequest accessRequest)
    {
        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync();
        return accessRequest;
    }

    public async Task<AccessRequest> UpdateAsync(AccessRequest accessRequest)
    {
        _context.AccessRequests.Update(accessRequest);
        await _context.SaveChangesAsync();
        return accessRequest;
    }
}

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}

public class DocumentRepository(ApplicationDbContext context) : IDocumentRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        return await _context.Documents.ToListAsync();
    }

    public async Task<Document?> GetByIdAsync(int id)
    {
        return await _context.Documents.FindAsync(id);
    }

    public async Task<Document> CreateAsync(Document document)
    {
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }
}

public class DecisionRepository(ApplicationDbContext context) : IDecisionRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Decision> CreateAsync(Decision decision)
    {
        _context.Decisions.Add(decision);
        await _context.SaveChangesAsync();
        return decision;
    }

    public async Task<Decision?> GetByAccessRequestIdAsync(int accessRequestId)
    {
        return await _context.Decisions
            .Include(d => d.Approver)
            .FirstOrDefaultAsync(d => d.AccessRequestId == accessRequestId);
    }
}