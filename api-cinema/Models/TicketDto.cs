using System.ComponentModel.DataAnnotations;

namespace api_cinema.Models;

public class PurchaseTicketDto
{
    [Required]
    public int ScreeningId { get; set; }
    
    [Required]
    [StringLength(10)]
    public string SeatNumber { get; set; } = string.Empty;
}

public class TicketResponseDto
{
    public int Id { get; set; }
    public int ScreeningId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string TheaterName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string Status { get; set; } = string.Empty;
}


