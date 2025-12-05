using System.ComponentModel.DataAnnotations;

namespace api_cinema.Models;

public class ScreeningDto
{
    [Required]
    public int MovieId { get; set; }
    
    [Required]
    public int TheaterId { get; set; }
    
    [Required]
    public DateTime ShowTime { get; set; }
    
    [Range(0.01, 1000)]
    public decimal Price { get; set; }
}

public class ScreeningResponseDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string? MoviePosterUrl { get; set; }
    public int TheaterId { get; set; }
    public string TheaterName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
    public int TotalSeats { get; set; }
    public bool IsActive { get; set; }
}


