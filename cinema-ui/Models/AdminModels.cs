using System.ComponentModel.DataAnnotations;
using cinema_ui.Services;

namespace cinema_ui.Models;

public class AdminDashboardViewModel
{
    public int TotalMovies { get; set; }
    public int ActiveMovies { get; set; }
    public int TotalTheaters { get; set; }
    public int ActiveTheaters { get; set; }
    public int TotalScreenings { get; set; }
    public int ActiveScreenings { get; set; }
    public int TotalUsers { get; set; }
    public int AdminUsers { get; set; }
}

public class AdminMovieViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Genre is required")]
    [StringLength(50)]
    public string Genre { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Duration is required")]
    [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes")]
    public int DurationMinutes { get; set; }
    
    [Required(ErrorMessage = "Release date is required")]
    [DataType(DataType.Date)]
    public DateTime ReleaseDate { get; set; } = DateTime.Now;
    
    [StringLength(100)]
    public string Director { get; set; } = string.Empty;
    
    [Url(ErrorMessage = "Invalid URL")]
    public string? PosterUrl { get; set; }
    
    [StringLength(10)]
    public string Rating { get; set; } = "NR";
    
    public bool IsActive { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

public class AdminTheaterViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Capacity is required")]
    [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000")]
    public int Capacity { get; set; }
    
    [Required(ErrorMessage = "Rows is required")]
    [Range(1, 50, ErrorMessage = "Rows must be between 1 and 50")]
    public int Rows { get; set; }
    
    [Required(ErrorMessage = "Seats per row is required")]
    [Range(1, 50, ErrorMessage = "Seats per row must be between 1 and 50")]
    public int SeatsPerRow { get; set; }
    
    public bool IsActive { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

public class AdminScreeningViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Movie is required")]
    public int MovieId { get; set; }
    
    [Required(ErrorMessage = "Theater is required")]
    public int TheaterId { get; set; }
    
    [Required(ErrorMessage = "Show time is required")]
    [DataType(DataType.DateTime)]
    public DateTime ShowTime { get; set; } = DateTime.Now.AddDays(1);
    
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 1000, ErrorMessage = "Price must be between 0.01 and 1000")]
    public decimal Price { get; set; }
    
    public List<MovieResponseDto> AvailableMovies { get; set; } = new();
    public List<TheaterResponseDto> AvailableTheaters { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class AdminScreeningsViewModel
{
    public List<ScreeningResponseDto> Screenings { get; set; } = new();
    public List<MovieResponseDto> Movies { get; set; } = new();
    public List<TheaterResponseDto> Theaters { get; set; } = new();
}

