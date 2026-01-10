namespace api_cinema.Models;

public class TicketNote
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int AdminUserId { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Ticket Ticket { get; set; } = null!;
    public User AdminUser { get; set; } = null!;
}
