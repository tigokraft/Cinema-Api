namespace api_cinema.Models;

/// <summary>
/// Represents a screening schedule that defines recurring screenings for a movie in a specific room.
/// Individual Screening records are generated from this schedule.
/// </summary>
public class ScreeningSchedule
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public int TheaterId { get; set; }
    public int RoomId { get; set; }
    
    // Date range when this schedule is active
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Show times during each day (stored as JSON array of times, e.g., ["10:00", "14:00", "18:00", "21:00"])
    public string ShowTimes { get; set; } = "[]";
    
    // Days of week when screenings occur (stored as comma-separated: "0,1,2,3,4,5,6" where 0=Sunday)
    public string DaysOfWeek { get; set; } = "0,1,2,3,4,5,6"; // Default: every day
    
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Movie Movie { get; set; } = null!;
    public Theater Theater { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public ICollection<Screening> Screenings { get; set; } = new List<Screening>();
}
