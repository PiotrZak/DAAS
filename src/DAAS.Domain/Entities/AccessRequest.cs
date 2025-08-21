namespace DAAS.Domain.Entities;

public class AccessRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int DocumentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public AccessType AccessType { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public DateTime RequestedAt { get; set; }
    
    public User User { get; set; } = null!;
    public Document Document { get; set; } = null!;
    public Decision? Decision { get; set; }
}

public enum AccessType
{
    Read = 0,
    Edit = 1
}

public enum RequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}