using api_cinema.Data;
using api_cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace api_cinema.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Authenticated")]
public class TicketController : ControllerBase
{
    private readonly AppDbContext _db;

    public TicketController(AppDbContext db)
    {
        _db = db;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    // POST: api/Ticket/purchase - Purchase a ticket
    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseTicket(PurchaseTicketDto dto)
    {
        var userId = GetUserId();
        
        var screening = await _db.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Theater)
            .Include(s => s.Tickets)
            .FirstOrDefaultAsync(s => s.Id == dto.ScreeningId && s.IsActive);

        if (screening == null)
            return NotFound("Screening not found.");

        if (screening.ShowTime <= DateTime.UtcNow)
            return BadRequest("Cannot purchase ticket for a past screening.");

        // Validate seat number format (e.g., "A1", "B5")
        if (!IsValidSeatNumber(dto.SeatNumber, screening.Theater.Rows, screening.Theater.SeatsPerRow))
            return BadRequest("Invalid seat number format or out of range.");

        // Check if seat is already taken
        var seatTaken = await _db.Tickets
            .AnyAsync(t => t.ScreeningId == dto.ScreeningId && 
                          t.SeatNumber == dto.SeatNumber && 
                          t.Status == "Active");

        if (seatTaken)
            return BadRequest("Seat is already taken.");

        // Check if user already has a ticket for this screening
        var userHasTicket = await _db.Tickets
            .AnyAsync(t => t.ScreeningId == dto.ScreeningId && 
                          t.UserId == userId && 
                          t.Status == "Active");

        if (userHasTicket)
            return BadRequest("You already have a ticket for this screening.");

        var ticket = new Ticket
        {
            ScreeningId = dto.ScreeningId,
            UserId = userId,
            SeatNumber = dto.SeatNumber,
            Price = screening.Price,
            PurchaseDate = DateTime.UtcNow,
            Status = "Active"
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        return Ok(new TicketResponseDto
        {
            Id = ticket.Id,
            ScreeningId = ticket.ScreeningId,
            MovieTitle = screening.Movie.Title,
            TheaterName = screening.Theater.Name,
            ShowTime = screening.ShowTime,
            SeatNumber = ticket.SeatNumber,
            Price = ticket.Price,
            PurchaseDate = ticket.PurchaseDate,
            Status = ticket.Status
        });
    }

    // GET: api/Ticket/my-tickets - Get user's tickets
    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetMyTickets([FromQuery] string? status)
    {
        var userId = GetUserId();

        var query = _db.Tickets
            .Include(t => t.Screening)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Screening)
                .ThenInclude(s => s!.Theater)
            .Where(t => t.UserId == userId);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);

        var tickets = await query
            .OrderByDescending(t => t.PurchaseDate)
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                ScreeningId = t.ScreeningId,
                MovieTitle = t.Screening!.Movie.Title,
                TheaterName = t.Screening.Theater.Name,
                ShowTime = t.Screening.ShowTime,
                SeatNumber = t.SeatNumber,
                Price = t.Price,
                PurchaseDate = t.PurchaseDate,
                Status = t.Status
            })
            .ToListAsync();

        return Ok(tickets);
    }

    // GET: api/Ticket/{id} - Get specific ticket
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicket(int id)
    {
        var userId = GetUserId();

        var ticket = await _db.Tickets
            .Include(t => t.Screening)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Screening)
                .ThenInclude(s => s!.Theater)
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                ScreeningId = t.ScreeningId,
                MovieTitle = t.Screening!.Movie.Title,
                TheaterName = t.Screening.Theater.Name,
                ShowTime = t.Screening.ShowTime,
                SeatNumber = t.SeatNumber,
                Price = t.Price,
                PurchaseDate = t.PurchaseDate,
                Status = t.Status
            })
            .FirstOrDefaultAsync();

        if (ticket == null)
            return NotFound("Ticket not found.");

        return Ok(ticket);
    }

    // POST: api/Ticket/{id}/cancel - Cancel a ticket
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelTicket(int id)
    {
        var userId = GetUserId();

        var ticket = await _db.Tickets
            .Include(t => t.Screening)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (ticket == null)
            return NotFound("Ticket not found.");

        if (ticket.Status != "Active")
            return BadRequest("Ticket is not active and cannot be cancelled.");

        if (ticket.Screening!.ShowTime <= DateTime.UtcNow)
            return BadRequest("Cannot cancel ticket for a past screening.");

        ticket.Status = "Cancelled";
        await _db.SaveChangesAsync();

        return Ok("Ticket cancelled successfully.");
    }

    // GET: api/Ticket/all - Admin only: Get all tickets
    [HttpGet("all")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAllTickets([FromQuery] string? status, [FromQuery] int? screeningId)
    {
        var query = _db.Tickets
            .Include(t => t.Screening)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Screening)
                .ThenInclude(s => s!.Theater)
            .Include(t => t.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);

        if (screeningId.HasValue)
            query = query.Where(t => t.ScreeningId == screeningId.Value);

        var tickets = await query
            .OrderByDescending(t => t.PurchaseDate)
            .Select(t => new
            {
                t.Id,
                t.ScreeningId,
                MovieTitle = t.Screening!.Movie.Title,
                TheaterName = t.Screening.Theater.Name,
                t.Screening.ShowTime,
                t.SeatNumber,
                t.Price,
                t.PurchaseDate,
                t.Status,
                UserId = t.UserId,
                Username = t.User.Username,
                UserEmail = t.User.Email
            })
            .ToListAsync();

        return Ok(tickets);
    }

    // PUT: api/Ticket/{id} - Admin only: Update ticket
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTicketDto dto)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound("Ticket not found.");

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!new[] { "Active", "Cancelled", "Used" }.Contains(dto.Status))
                return BadRequest("Invalid status. Must be 'Active', 'Cancelled', or 'Used'.");
            ticket.Status = dto.Status;
        }

        if (!string.IsNullOrWhiteSpace(dto.SeatNumber))
        {
            var screening = await _db.Screenings
                .Include(s => s.Theater)
                .FirstOrDefaultAsync(s => s.Id == ticket.ScreeningId);

            if (screening == null)
                return BadRequest("Screening not found.");

            if (!IsValidSeatNumber(dto.SeatNumber, screening.Theater.Rows, screening.Theater.SeatsPerRow))
                return BadRequest("Invalid seat number format or out of range.");

            // Check if new seat is already taken
            var seatTaken = await _db.Tickets
                .AnyAsync(t => t.ScreeningId == ticket.ScreeningId && 
                              t.SeatNumber == dto.SeatNumber && 
                              t.Status == "Active" &&
                              t.Id != id);

            if (seatTaken)
                return BadRequest("Seat is already taken.");

            ticket.SeatNumber = dto.SeatNumber;
        }

        await _db.SaveChangesAsync();

        var updatedTicket = await _db.Tickets
            .Include(t => t.Screening)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Screening)
                .ThenInclude(s => s!.Theater)
            .Where(t => t.Id == id)
            .Select(t => new TicketResponseDto
            {
                Id = t.Id,
                ScreeningId = t.ScreeningId,
                MovieTitle = t.Screening!.Movie.Title,
                TheaterName = t.Screening.Theater.Name,
                ShowTime = t.Screening.ShowTime,
                SeatNumber = t.SeatNumber,
                Price = t.Price,
                PurchaseDate = t.PurchaseDate,
                Status = t.Status
            })
            .FirstOrDefaultAsync();

        return Ok(updatedTicket);
    }

    // Helper method to validate seat number
    private static bool IsValidSeatNumber(string seatNumber, int rows, int seatsPerRow)
    {
        if (string.IsNullOrWhiteSpace(seatNumber) || seatNumber.Length < 2)
            return false;

        var rowChar = seatNumber[0];
        if (!char.IsLetter(rowChar))
            return false;

        var rowIndex = char.ToUpper(rowChar) - 'A';
        if (rowIndex < 0 || rowIndex >= rows)
            return false;

        if (!int.TryParse(seatNumber.Substring(1), out var seatIndex))
            return false;

        return seatIndex >= 1 && seatIndex <= seatsPerRow;
    }
}

public class UpdateTicketDto
{
    public string? Status { get; set; }
    public string? SeatNumber { get; set; }
}


