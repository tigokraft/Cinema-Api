namespace api_cinema.Models;

public class Ticket
{
    public int Id { get; set; }
    public int ScreeningId { get; set; }
    public int UserId { get; set; }
    public string SeatNumber { get; set; } = string.Empty; // e.g., "A1", "B5"
    public decimal Price { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Active"; // Active, Cancelled, Used
    
    // Navigation properties
    public Screening Screening { get; set; } = null!;
    public User User { get; set; } = null!;
}


