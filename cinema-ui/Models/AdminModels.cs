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

public class EnhancedDashboardViewModel : AdminDashboardViewModel
{
    public RevenueMetricsDto? Revenue { get; set; }
    public TicketStatisticsDto? Tickets { get; set; }
    public List<PopularMovieDto>? PopularMovies { get; set; }
    public List<ActivityFeedItemDto>? ActivityFeed { get; set; }
    public List<AlertDto>? Alerts { get; set; }
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
    
    [Url(ErrorMessage = "Invalid URL")]
    public string? BackdropUrl { get; set; }
    
    [Url(ErrorMessage = "Invalid URL")]
    public string? TrailerUrl { get; set; }
    
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
    
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Number of rooms is required")]
    [Range(1, 50, ErrorMessage = "Number of rooms must be between 1 and 50")]
    public int RoomCount { get; set; } = 1;
    
    [Required(ErrorMessage = "Rows is required")]
    [Range(1, 50, ErrorMessage = "Rows must be between 1 and 50")]
    public int Rows { get; set; }
    
    [Required(ErrorMessage = "Seats per row is required")]
    [Range(1, 50, ErrorMessage = "Seats per row must be between 1 and 50")]
    public int SeatsPerRow { get; set; }
    
    public int Capacity => Rows * SeatsPerRow;
    
    public bool IsActive { get; set; } = true;
    public List<RoomResponseDto> Rooms { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class AdminScreeningViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Movie is required")]
    public int MovieId { get; set; }
    
    [Required(ErrorMessage = "Theater is required")]
    public int TheaterId { get; set; }
    
    [Required(ErrorMessage = "Room is required")]
    public int RoomId { get; set; }
    
    [Required(ErrorMessage = "Start date is required")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Now.Date.AddDays(1);
    
    [Required(ErrorMessage = "End date is required")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Now.Date.AddDays(7);
    
    /// <summary>
    /// Show times as comma-separated values (e.g., "10:00,14:00,18:00,21:00")
    /// </summary>
    [Required(ErrorMessage = "At least one show time is required")]
    public string ShowTimesInput { get; set; } = "10:00,14:00,18:00";
    
    /// <summary>
    /// Selected days of week (0=Sunday, 1=Monday, ..., 6=Saturday)
    /// </summary>
    public List<int> SelectedDays { get; set; } = new() { 0, 1, 2, 3, 4, 5, 6 };
    
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 1000, ErrorMessage = "Price must be between 0.01 and 1000")]
    public decimal Price { get; set; }
    
    public List<MovieResponseDto> AvailableMovies { get; set; } = new();
    public List<TheaterResponseDto> AvailableTheaters { get; set; } = new();
    public List<RoomResponseDto> AvailableRooms { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
}

public class AdminScreeningsViewModel
{
    public List<ScreeningResponseDto> Screenings { get; set; } = new();
    public List<ScreeningScheduleResponseDto> Schedules { get; set; } = new();
    public List<MovieResponseDto> Movies { get; set; } = new();
    public List<TheaterResponseDto> Theaters { get; set; } = new();
}

public class AdminTicketsViewModel
{
    public List<cinema_ui.Services.AdminTicketDto> Tickets { get; set; } = new();
    public List<cinema_ui.Services.ScreeningResponseDto> Screenings { get; set; } = new();
    public string? SelectedStatus { get; set; }
    public int? SelectedScreeningId { get; set; }
}

public class AdminEditTicketViewModel
{
    public int Id { get; set; }
    public int ScreeningId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string TheaterName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    
    [Required]
    [StringLength(10)]
    public string SeatNumber { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    
    [Required]
    public string Status { get; set; } = "Active";
    
    public string Username { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TmdbImportViewModel
{
    public string SearchQuery { get; set; } = string.Empty;
    public List<TmdbMovieDto> SearchResults { get; set; } = new();
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 0;
    public int TotalResults { get; set; } = 0;
    public string? ErrorMessage { get; set; }
}

public class TmdbMovieDetailsViewModel
{
    public int TmdbId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Overview { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public DateTime ReleaseDate { get; set; }
    public int Runtime { get; set; }
    public List<string> Genres { get; set; } = new();
    public string Director { get; set; } = string.Empty;
}

public class AdminEditUserViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }
    
    [StringLength(50)]
    public string? FirstName { get; set; }
    
    [StringLength(50)]
    public string? LastName { get; set; }
    
    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = "User";
    
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string? NewPassword { get; set; }
    
    public int TicketCount { get; set; }
    public string? ErrorMessage { get; set; }
}

// Reports View Model
public class ReportsViewModel
{
    public RevenueMetricsDto? Revenue { get; set; }
    public TicketStatisticsDto? Tickets { get; set; }
    public List<PopularMovieDto>? PopularMovies { get; set; }
    public List<PeakHoursDto>? PeakHours { get; set; }
    public List<OccupancyRateDto>? OccupancyRates { get; set; }
    public int SelectedDays { get; set; } = 30;
}

// Promo Code Form View Model
public class PromoCodeFormViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Code is required")]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Discount percent is required")]
    [Range(1, 100, ErrorMessage = "Discount must be between 1 and 100")]
    public decimal DiscountPercent { get; set; }
    
    public decimal? MaxDiscountAmount { get; set; }
    public int? MaxUses { get; set; }
    public decimal? MinPurchaseAmount { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? ValidFrom { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime? ExpiresAt { get; set; }
    
    public string? ErrorMessage { get; set; }
}

// Audit Logs View Model
public class AuditLogsViewModel
{
    public List<AuditLogResponseDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public List<string> EntityTypes { get; set; } = new();
    public List<string> Actions { get; set; } = new();
    public string? SelectedAction { get; set; }
    public string? SelectedEntityType { get; set; }
}
