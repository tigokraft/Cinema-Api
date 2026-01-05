using System.ComponentModel.DataAnnotations;

namespace api_cinema.Models;

public class RoomDto
{
    [Required]
    public int TheaterId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Range(1, 100)]
    public int RoomNumber { get; set; }
}

public class RoomResponseDto
{
    public int Id { get; set; }
    public int TheaterId { get; set; }
    public string TheaterName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int RoomNumber { get; set; }
    public int Capacity { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
    public bool IsActive { get; set; }
}
