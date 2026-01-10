namespace api_cinema.Models;

public class AuditLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE
    public string EntityType { get; set; } = string.Empty; // Movie, Ticket, User, etc.
    public int EntityId { get; set; }
    public string? Details { get; set; } // JSON with change details
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public User User { get; set; } = null!;
}
