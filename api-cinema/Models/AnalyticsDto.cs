namespace api_cinema.Models;

// Analytics DTOs
public class RevenueMetricsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public List<RevenueByDateDto> DailyRevenue { get; set; } = new();
    public List<RevenueByMovieDto> TopMoviesByRevenue { get; set; } = new();
    public List<RevenueByTheaterDto> RevenueByTheater { get; set; } = new();
}

public class RevenueByDateDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int TicketCount { get; set; }
}

public class RevenueByMovieDto
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public decimal Revenue { get; set; }
    public int TicketCount { get; set; }
}

public class RevenueByTheaterDto
{
    public int TheaterId { get; set; }
    public string TheaterName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int TicketCount { get; set; }
}

public class TicketStatisticsDto
{
    public int TotalTickets { get; set; }
    public int ActiveTickets { get; set; }
    public int UsedTickets { get; set; }
    public int CancelledTickets { get; set; }
    public int TodayTickets { get; set; }
    public int WeekTickets { get; set; }
    public int MonthTickets { get; set; }
    public List<TicketsByDateDto> DailyTickets { get; set; } = new();
}

public class TicketsByDateDto
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class OccupancyRateDto
{
    public int ScreeningId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string TheaterName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public int TotalSeats { get; set; }
    public int SoldSeats { get; set; }
    public decimal OccupancyPercent { get; set; }
}

public class PopularMovieDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
    public int ScreeningsCount { get; set; }
    public decimal AverageOccupancy { get; set; }
}

public class PeakHoursDto
{
    public int Hour { get; set; }
    public int TicketCount { get; set; }
    public decimal RevenueAmount { get; set; }
}

public class ActivityFeedItemDto
{
    public string Type { get; set; } = string.Empty; // Purchase, Cancellation, Registration, etc.
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Username { get; set; }
    public int? EntityId { get; set; }
}

// Promo Code DTOs
public class PromoCodeDto
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? MaxUses { get; set; }
    public decimal? MinPurchaseAmount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class PromoCodeResponseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public decimal? MinPurchaseAmount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class PromoCodeValidationDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
}

// Audit Log DTOs
public class AuditLogResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
}

// Ticket Note DTOs
public class TicketNoteDto
{
    public string Note { get; set; } = string.Empty;
}

public class TicketNoteResponseDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string Note { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Advanced Ticket DTOs
public class AdminTicketDetailDto
{
    public int Id { get; set; }
    public int ScreeningId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string TheaterName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CheckedInAt { get; set; }
    public string? RefundReason { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? PromoCode { get; set; }
    public List<TicketNoteResponseDto> Notes { get; set; } = new();
}

public class BulkTicketOperationDto
{
    public List<int> TicketIds { get; set; } = new();
    public string? Reason { get; set; }
}

public class TicketSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public int? ScreeningId { get; set; }
    public int? MovieId { get; set; }
    public int? TheaterId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class TicketSearchResultDto
{
    public List<AdminTicketDetailDto> Tickets { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
