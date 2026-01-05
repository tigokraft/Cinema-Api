namespace api_cinema.Models;

public class Theater
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    
    // Room configuration - all rooms in this theater have the same size
    public int RoomCount { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
    public int Capacity => Rows * SeatsPerRow; // Per room capacity
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<Screening> Screenings { get; set; } = new List<Screening>();
}
