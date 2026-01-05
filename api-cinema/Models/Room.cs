namespace api_cinema.Models;

public class Room
{
    public int Id { get; set; }
    public int TheaterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RoomNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Theater Theater { get; set; } = null!;
    public ICollection<Screening> Screenings { get; set; } = new List<Screening>();
}
