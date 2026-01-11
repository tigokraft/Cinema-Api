using api_cinema.Data;
using api_cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

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

        // Apply promo code if provided
        if (!string.IsNullOrWhiteSpace(dto.PromoCode))
        {
            var promoCode = await _db.PromoCodes
                .FirstOrDefaultAsync(p => p.Code == dto.PromoCode.ToUpper() && p.IsActive);
            
            if (promoCode != null)
            {
                // Validate promo code
                var isValid = true;
                if (promoCode.ValidFrom.HasValue && promoCode.ValidFrom > DateTime.UtcNow) isValid = false;
                if (promoCode.ExpiresAt.HasValue && promoCode.ExpiresAt < DateTime.UtcNow) isValid = false;
                if (promoCode.MaxUses.HasValue && promoCode.CurrentUses >= promoCode.MaxUses) isValid = false;
                if (promoCode.MinPurchaseAmount.HasValue && screening.Price < promoCode.MinPurchaseAmount) isValid = false;

                if (isValid)
                {
                    var discount = screening.Price * (promoCode.DiscountPercent / 100);
                    if (promoCode.MaxDiscountAmount.HasValue && discount > promoCode.MaxDiscountAmount)
                        discount = promoCode.MaxDiscountAmount.Value;
                    
                    ticket.PromoCodeId = promoCode.Id;
                    ticket.DiscountAmount = discount;
                    promoCode.CurrentUses++;
                }
            }
        }

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

        // Check if screening is on current date (UTC)
        var screeningDate = ticket.Screening!.ShowTime.Date;
        var today = DateTime.UtcNow.Date;
        
        if (screeningDate <= today)
            return BadRequest("Cannot cancel tickets for screenings on the current date or in the past. Cancellations must be made at least one day before the screening.");

        ticket.Status = "Cancelled";
        ticket.RefundReason = "User requested cancellation";
        await _db.SaveChangesAsync();

        return Ok(new { Message = "Ticket cancelled successfully. A refund will be processed.", TicketId = id });
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
                t.CheckedInAt,
                UserId = t.UserId,
                Username = t.User.Username,
                UserEmail = t.User.Email
            })
            .ToListAsync();

        return Ok(tickets);
    }

    // GET: api/Ticket/search - Admin only: Advanced ticket search
    [HttpGet("search")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SearchTickets([FromQuery] TicketSearchDto search)
    {
        var query = _db.Tickets
            .Include(t => t.Screening)
                .ThenInclude(s => s!.Movie)
            .Include(t => t.Screening)
                .ThenInclude(s => s!.Theater)
            .Include(t => t.Screening)
                .ThenInclude(s => s!.Room)
            .Include(t => t.User)
            .Include(t => t.PromoCode)
            .Include(t => t.Notes)
                .ThenInclude(n => n.AdminUser)
            .AsQueryable();

        // Search term (email, username, ticket ID)
        if (!string.IsNullOrWhiteSpace(search.SearchTerm))
        {
            var term = search.SearchTerm.ToLower();
            if (int.TryParse(term, out var ticketId))
            {
                query = query.Where(t => t.Id == ticketId);
            }
            else
            {
                query = query.Where(t => 
                    t.User.Email!.ToLower().Contains(term) ||
                    t.User.Username.ToLower().Contains(term) ||
                    t.Screening.Movie.Title.ToLower().Contains(term));
            }
        }

        if (!string.IsNullOrWhiteSpace(search.Status))
            query = query.Where(t => t.Status == search.Status);

        if (search.ScreeningId.HasValue)
            query = query.Where(t => t.ScreeningId == search.ScreeningId.Value);

        if (search.MovieId.HasValue)
            query = query.Where(t => t.Screening.MovieId == search.MovieId.Value);

        if (search.TheaterId.HasValue)
            query = query.Where(t => t.Screening.TheaterId == search.TheaterId.Value);

        if (search.DateFrom.HasValue)
            query = query.Where(t => t.PurchaseDate >= search.DateFrom.Value);

        if (search.DateTo.HasValue)
            query = query.Where(t => t.PurchaseDate <= search.DateTo.Value);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / search.PageSize);

        var tickets = await query
            .OrderByDescending(t => t.PurchaseDate)
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .Select(t => new AdminTicketDetailDto
            {
                Id = t.Id,
                ScreeningId = t.ScreeningId,
                MovieTitle = t.Screening!.Movie.Title,
                TheaterName = t.Screening.Theater.Name,
                RoomName = t.Screening.Room != null ? t.Screening.Room.Name : "Main",
                ShowTime = t.Screening.ShowTime,
                SeatNumber = t.SeatNumber,
                Price = t.Price,
                DiscountAmount = t.DiscountAmount,
                PurchaseDate = t.PurchaseDate,
                Status = t.Status,
                CheckedInAt = t.CheckedInAt,
                RefundReason = t.RefundReason,
                UserId = t.UserId,
                Username = t.User.Username,
                UserEmail = t.User.Email,
                PromoCode = t.PromoCode != null ? t.PromoCode.Code : null,
                Notes = t.Notes.Select(n => new TicketNoteResponseDto
                {
                    Id = n.Id,
                    TicketId = n.TicketId,
                    Note = n.Note,
                    AdminUsername = n.AdminUser.Username,
                    CreatedAt = n.CreatedAt
                }).ToList()
            })
            .ToListAsync();

        return Ok(new TicketSearchResultDto
        {
            Tickets = tickets,
            TotalCount = totalCount,
            Page = search.Page,
            PageSize = search.PageSize,
            TotalPages = totalPages
        });
    }

    // POST: api/Ticket/bulk-cancel - Admin only: Cancel multiple tickets
    [HttpPost("bulk-cancel")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> BulkCancel([FromBody] BulkTicketOperationDto dto)
    {
        if (dto.TicketIds == null || !dto.TicketIds.Any())
            return BadRequest("No ticket IDs provided.");

        var tickets = await _db.Tickets
            .Where(t => dto.TicketIds.Contains(t.Id) && t.Status == "Active")
            .ToListAsync();

        if (!tickets.Any())
            return BadRequest("No active tickets found with the provided IDs.");

        foreach (var ticket in tickets)
        {
            ticket.Status = "Cancelled";
            ticket.RefundReason = dto.Reason ?? "Bulk cancellation by admin";
        }

        await _db.SaveChangesAsync();

        return Ok(new { Message = $"{tickets.Count} tickets cancelled successfully.", CancelledCount = tickets.Count });
    }

    // POST: api/Ticket/bulk-mark-used - Admin only: Mark multiple tickets as used
    [HttpPost("bulk-mark-used")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> BulkMarkUsed([FromBody] BulkTicketOperationDto dto)
    {
        if (dto.TicketIds == null || !dto.TicketIds.Any())
            return BadRequest("No ticket IDs provided.");

        var tickets = await _db.Tickets
            .Where(t => dto.TicketIds.Contains(t.Id) && t.Status == "Active")
            .ToListAsync();

        if (!tickets.Any())
            return BadRequest("No active tickets found with the provided IDs.");

        foreach (var ticket in tickets)
        {
            ticket.Status = "Used";
            ticket.CheckedInAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return Ok(new { Message = $"{tickets.Count} tickets marked as used.", UpdatedCount = tickets.Count });
    }

    // POST: api/Ticket/{id}/check-in - Admin only: Check in a single ticket
    [HttpPost("{id}/check-in")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CheckIn(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound("Ticket not found.");

        if (ticket.Status != "Active")
            return BadRequest($"Cannot check in ticket with status '{ticket.Status}'.");

        if (ticket.CheckedInAt.HasValue)
            return BadRequest("Ticket has already been checked in.");

        ticket.CheckedInAt = DateTime.UtcNow;
        ticket.Status = "Used";
        await _db.SaveChangesAsync();

        return Ok(new { Message = "Ticket checked in successfully.", CheckedInAt = ticket.CheckedInAt });
    }

    // POST: api/Ticket/{id}/notes - Admin only: Add a note to a ticket
    [HttpPost("{id}/notes")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AddNote(int id, [FromBody] TicketNoteDto dto)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound("Ticket not found.");

        if (string.IsNullOrWhiteSpace(dto.Note))
            return BadRequest("Note cannot be empty.");

        var note = new TicketNote
        {
            TicketId = id,
            AdminUserId = GetUserId(),
            Note = dto.Note
        };

        _db.TicketNotes.Add(note);
        await _db.SaveChangesAsync();

        var adminUser = await _db.Users.FindAsync(GetUserId());

        return Ok(new TicketNoteResponseDto
        {
            Id = note.Id,
            TicketId = note.TicketId,
            Note = note.Note,
            AdminUsername = adminUser?.Username ?? "Unknown",
            CreatedAt = note.CreatedAt
        });
    }

    // GET: api/Ticket/{id}/notes - Admin only: Get all notes for a ticket
    [HttpGet("{id}/notes")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetNotes(int id)
    {
        var ticket = await _db.Tickets.FindAsync(id);
        if (ticket == null)
            return NotFound("Ticket not found.");

        var notes = await _db.TicketNotes
            .Include(n => n.AdminUser)
            .Where(n => n.TicketId == id)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new TicketNoteResponseDto
            {
                Id = n.Id,
                TicketId = n.TicketId,
                Note = n.Note,
                AdminUsername = n.AdminUser.Username,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return Ok(notes);
    }

    // GET: api/Ticket/export - Admin only: Export tickets to CSV
    [HttpGet("export")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ExportTickets(
        [FromQuery] string? status,
        [FromQuery] int? screeningId,
        [FromQuery] int? movieId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
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

        if (movieId.HasValue)
            query = query.Where(t => t.Screening.MovieId == movieId.Value);

        if (dateFrom.HasValue)
            query = query.Where(t => t.PurchaseDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(t => t.PurchaseDate <= dateTo.Value);

        var tickets = await query
            .OrderByDescending(t => t.PurchaseDate)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("ID,Movie,Theater,ShowTime,Seat,Price,Discount,FinalPrice,Status,User,Email,PurchaseDate,CheckedInAt");

        foreach (var t in tickets)
        {
            var finalPrice = t.Price - (t.DiscountAmount ?? 0);
            csv.AppendLine($"{t.Id},\"{t.Screening.Movie.Title}\",\"{t.Screening.Theater.Name}\",{t.Screening.ShowTime:yyyy-MM-dd HH:mm},{t.SeatNumber},{t.Price:F2},{t.DiscountAmount?.ToString("F2") ?? "0.00"},{finalPrice:F2},{t.Status},\"{t.User.Username}\",{t.User.Email ?? ""},{t.PurchaseDate:yyyy-MM-dd HH:mm},{t.CheckedInAt?.ToString("yyyy-MM-dd HH:mm") ?? ""}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"tickets_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
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
            
            if (dto.Status == "Cancelled" && ticket.Status != "Cancelled")
                ticket.RefundReason = dto.RefundReason ?? "Cancelled by admin";
            
            if (dto.Status == "Used" && !ticket.CheckedInAt.HasValue)
                ticket.CheckedInAt = DateTime.UtcNow;
            
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

public class PurchaseTicketDto
{
    public int ScreeningId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string? PromoCode { get; set; }
}

public class UpdateTicketDto
{
    public string? Status { get; set; }
    public string? SeatNumber { get; set; }
    public string? RefundReason { get; set; }
}
