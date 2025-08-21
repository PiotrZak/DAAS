namespace DAAS.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    
    public ICollection<AccessRequest> AccessRequests { get; set; } = new List<AccessRequest>();
    public ICollection<Decision> Decisions { get; set; } = new List<Decision>();
}

public enum UserRole
{
    User = 0,
    Approver = 1,
    Admin = 2
}
