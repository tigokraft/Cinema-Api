using api_cinema.Data;
using api_cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_cinema.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreeningController : ControllerBase
{
    private readonly AppDbContext _db;

    public ScreeningController(AppDbContext db)
    {
        _db = db;
    }

    private string? EnsureValidImageUrl(string? posterUrl)
    {
        if (string.IsNullOrWhiteSpace(posterUrl))
            return null;

        // If it's already a full URL starting with https://image.tmdb.org, return as is
        if (posterUrl.StartsWith("https://image.tmdb.org", StringComparison.OrdinalIgnoreCase) || 
            posterUrl.StartsWith("http://image.tmdb.org", StringComparison.OrdinalIgnoreCase))
            return posterUrl;

        // If it's already a full URL but from another source, return as is
        if (posterUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
            posterUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return posterUrl;

        // Remove any leading/trailing whitespace
        var cleanPath = posterUrl.Trim();

        // If it's a partial path (starts with /), construct full TMDB URL
        if (cleanPath.StartsWith("/"))
        {
            // Ensure we have the correct format: /path/to/image.jpg
            return $"https://image.tmdb.org/t/p/w780{cleanPath}";
        }

        // If it doesn't start with /, add it
        return $"https://image.tmdb.org/t/p/w780/{cleanPath}";
    }

    // GET: api/Screening - Public endpoint with availability
    [HttpGet]
    public async Task<IActionResult> GetScreenings([FromQuery] int? movieId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var query = _db.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Theater)
            .Where(s => s.IsActive && s.ShowTime >= DateTime.UtcNow);

        if (movieId.HasValue)
            query = query.Where(s => s.MovieId == movieId.Value);

        if (fromDate.HasValue)
            query = query.Where(s => s.ShowTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.ShowTime <= toDate.Value);

        var screenings = await query
            .OrderBy(s => s.ShowTime)
            .ToListAsync();

        var screeningDtos = screenings.Select(s => new ScreeningResponseDto
        {
            Id = s.Id,
            MovieId = s.MovieId,
            MovieTitle = s.Movie.Title,
            MoviePosterUrl = EnsureValidImageUrl(s.Movie.PosterUrl),
            TheaterId = s.TheaterId,
            TheaterName = s.Theater.Name,
            ShowTime = s.ShowTime,
            Price = s.Price,
            TotalSeats = s.Theater.Capacity,
            AvailableSeats = s.Theater.Capacity - s.Tickets.Count(t => t.Status == "Active"),
            IsActive = s.IsActive
        }).ToList();

        return Ok(screeningDtos);
    }

    // GET: api/Screening/{id} - Public endpoint with seat availability
    [HttpGet("{id}")]
    public async Task<IActionResult> GetScreening(int id)
    {
        var screening = await _db.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Theater)
            .Include(s => s.Tickets)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (screening == null)
            return NotFound("Screening not found.");

        var occupiedSeats = screening.Tickets
            .Where(t => t.Status == "Active")
            .Select(t => t.SeatNumber)
            .ToList();

        var result = new
        {
            Id = screening.Id,
            MovieId = screening.MovieId,
            MovieTitle = screening.Movie.Title,
            MoviePosterUrl = EnsureValidImageUrl(screening.Movie.PosterUrl),
            TheaterId = screening.TheaterId,
            TheaterName = screening.Theater.Name,
            ShowTime = screening.ShowTime,
            Price = screening.Price,
            TotalSeats = screening.Theater.Capacity,
            AvailableSeats = screening.Theater.Capacity - occupiedSeats.Count,
            Rows = screening.Theater.Rows,
            SeatsPerRow = screening.Theater.SeatsPerRow,
            OccupiedSeats = occupiedSeats
        };

        return Ok(result);
    }

    // GET: api/Screening/upcoming - Public endpoint for upcoming screenings
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingScreenings([FromQuery] int limit = 10)
    {
        var screenings = await _db.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Theater)
            .Include(s => s.Tickets)
            .Where(s => s.IsActive && s.ShowTime >= DateTime.UtcNow)
            .OrderBy(s => s.ShowTime)
            .Take(limit)
            .ToListAsync();

        var screeningDtos = screenings.Select(s => new ScreeningResponseDto
        {
            Id = s.Id,
            MovieId = s.MovieId,
            MovieTitle = s.Movie.Title,
            MoviePosterUrl = EnsureValidImageUrl(s.Movie.PosterUrl),
            TheaterId = s.TheaterId,
            TheaterName = s.Theater.Name,
            ShowTime = s.ShowTime,
            Price = s.Price,
            TotalSeats = s.Theater.Capacity,
            AvailableSeats = s.Theater.Capacity - s.Tickets.Count(t => t.Status == "Active"),
            IsActive = s.IsActive
        }).ToList();

        return Ok(screeningDtos);
    }

    // POST: api/Screening - Admin only
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateScreening(ScreeningDto dto)
    {
        var movie = await _db.Movies.FindAsync(dto.MovieId);
        if (movie == null)
            return NotFound("Movie not found.");

        var theater = await _db.Theaters.FindAsync(dto.TheaterId);
        if (theater == null)
            return NotFound("Theater not found.");

        if (!theater.IsActive)
            return BadRequest("Theater is not active.");

        if (dto.ShowTime <= DateTime.UtcNow)
            return BadRequest("Show time must be in the future.");

        // Check for overlapping screenings in the same theater
        var movieDuration = TimeSpan.FromMinutes(movie.DurationMinutes);
        var screeningEnd = dto.ShowTime.Add(movieDuration);
        
        var existingScreenings = await _db.Screenings
            .Include(s => s.Movie)
            .Where(s => s.TheaterId == dto.TheaterId && s.IsActive)
            .ToListAsync();

        var overlapping = existingScreenings.Any(s =>
        {
            var sEnd = s.ShowTime.Add(TimeSpan.FromMinutes(s.Movie.DurationMinutes));
            return (dto.ShowTime >= s.ShowTime && dto.ShowTime < sEnd) ||
                   (screeningEnd > s.ShowTime && screeningEnd <= sEnd) ||
                   (dto.ShowTime <= s.ShowTime && screeningEnd >= sEnd);
        });

        if (overlapping)
            return BadRequest("There is an overlapping screening in this theater.");

        // Ensure ShowTime is UTC (PostgreSQL requirement)
        var showTimeUtc = dto.ShowTime.Kind == DateTimeKind.Utc 
            ? dto.ShowTime 
            : DateTime.SpecifyKind(dto.ShowTime, DateTimeKind.Utc);

        var screening = new Screening
        {
            MovieId = dto.MovieId,
            TheaterId = dto.TheaterId,
            ShowTime = showTimeUtc,
            Price = dto.Price,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Screenings.Add(screening);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetScreening), new { id = screening.Id }, new ScreeningResponseDto
        {
            Id = screening.Id,
            MovieId = screening.MovieId,
            MovieTitle = movie.Title,
            MoviePosterUrl = EnsureValidImageUrl(movie.PosterUrl),
            TheaterId = screening.TheaterId,
            TheaterName = theater.Name,
            ShowTime = screening.ShowTime,
            Price = screening.Price,
            TotalSeats = theater.Capacity,
            AvailableSeats = theater.Capacity,
            IsActive = screening.IsActive
        });
    }

    // PUT: api/Screening/{id} - Admin only
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateScreening(int id, ScreeningDto dto)
    {
        var screening = await _db.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Theater)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (screening == null)
            return NotFound("Screening not found.");

        // Check if tickets have been sold
        var hasTickets = await _db.Tickets.AnyAsync(t => t.ScreeningId == id && t.Status == "Active");
        if (hasTickets)
            return BadRequest("Cannot update screening with active tickets.");

        var movie = await _db.Movies.FindAsync(dto.MovieId);
        if (movie == null)
            return NotFound("Movie not found.");

        var theater = await _db.Theaters.FindAsync(dto.TheaterId);
        if (theater == null)
            return NotFound("Theater not found.");

        // Ensure ShowTime is UTC (PostgreSQL requirement)
        var showTimeUtc = dto.ShowTime.Kind == DateTimeKind.Utc 
            ? dto.ShowTime 
            : DateTime.SpecifyKind(dto.ShowTime, DateTimeKind.Utc);

        screening.MovieId = dto.MovieId;
        screening.TheaterId = dto.TheaterId;
        screening.ShowTime = showTimeUtc;
        screening.Price = dto.Price;

        await _db.SaveChangesAsync();

        return Ok(new ScreeningResponseDto
        {
            Id = screening.Id,
            MovieId = screening.MovieId,
            MovieTitle = movie.Title,
            MoviePosterUrl = EnsureValidImageUrl(movie.PosterUrl),
            TheaterId = screening.TheaterId,
            TheaterName = theater.Name,
            ShowTime = screening.ShowTime,
            Price = screening.Price,
            TotalSeats = theater.Capacity,
            AvailableSeats = theater.Capacity,
            IsActive = screening.IsActive
        });
    }

    // DELETE: api/Screening/{id} - Admin only (soft delete)
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteScreening(int id)
    {
        var screening = await _db.Screenings.FindAsync(id);
        if (screening == null)
            return NotFound("Screening not found.");

        var hasTickets = await _db.Tickets.AnyAsync(t => t.ScreeningId == id && t.Status == "Active");
        if (hasTickets)
            return BadRequest("Cannot delete screening with active tickets. Cancel tickets first.");

        screening.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok("Screening deactivated successfully.");
    }
}


