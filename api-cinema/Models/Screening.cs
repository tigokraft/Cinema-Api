namespace api_cinema.Models;

public class Screening
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public int TheaterId { get; set; }
    public int RoomId { get; set; }
    public DateTime ShowTime { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Optional: Reference to the schedule that created this screening
    public int? ScreeningScheduleId { get; set; }
    
    // Navigation properties
    public Movie Movie { get; set; } = null!;
    public Theater Theater { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public ScreeningSchedule? ScreeningSchedule { get; set; }
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
