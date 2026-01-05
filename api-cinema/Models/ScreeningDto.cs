using System.ComponentModel.DataAnnotations;

namespace api_cinema.Models;

/// <summary>
/// DTO for creating a screening schedule (batch of recurring screenings)
/// </summary>
public class ScreeningScheduleDto
{
    [Required]
    public int MovieId { get; set; }
    
    [Required]
    public int TheaterId { get; set; }
    
    [Required]
    public int RoomId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// List of show times in HH:mm format, e.g., ["10:00", "14:00", "18:00", "21:00"]
    /// </summary>
    [Required]
    public List<string> ShowTimes { get; set; } = new();
    
    /// <summary>
    /// Days of week when screenings occur (0=Sunday, 1=Monday, ..., 6=Saturday)
    /// </summary>
    [Required]
    public List<int> DaysOfWeek { get; set; } = new() { 0, 1, 2, 3, 4, 5, 6 };
    
    [Range(0.01, 1000)]
    public decimal Price { get; set; }
}

/// <summary>
/// Legacy DTO for creating a single screening (kept for backward compatibility)
/// </summary>
public class ScreeningDto
{
    [Required]
    public int MovieId { get; set; }
    
    [Required]
    public int TheaterId { get; set; }
    
    [Required]
    public int RoomId { get; set; }
    
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
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int RoomNumber { get; set; }
    public DateTime ShowTime { get; set; }
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
    public int TotalSeats { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
    public bool IsActive { get; set; }
    public int? ScreeningScheduleId { get; set; }
}

public class ScreeningScheduleResponseDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string? MoviePosterUrl { get; set; }
    public int TheaterId { get; set; }
    public string TheaterName { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int RoomNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> ShowTimes { get; set; } = new();
    public List<int> DaysOfWeek { get; set; } = new();
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int ScreeningCount { get; set; }
}

/// <summary>
/// DTO for updating an existing screening schedule
/// </summary>
public class ScreeningScheduleUpdateDto
{
    public DateTime? EndDate { get; set; }
    public List<string>? ShowTimes { get; set; }
    public List<int>? DaysOfWeek { get; set; }
    public decimal? Price { get; set; }
}
