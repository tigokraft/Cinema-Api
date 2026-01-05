using api_cinema.Data;
using api_cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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

        if (posterUrl.StartsWith("https://image.tmdb.org", StringComparison.OrdinalIgnoreCase) || 
            posterUrl.StartsWith("http://image.tmdb.org", StringComparison.OrdinalIgnoreCase))
            return posterUrl;

        if (posterUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
            posterUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return posterUrl;

        var cleanPath = posterUrl.Trim();
        if (cleanPath.StartsWith("/"))
        {
            return $"https://image.tmdb.org/t/p/w780{cleanPath}";
        }

        return $"https://image.tmdb.org/t/p/w780/{cleanPath}";
    }

    // GET: api/Screening - Public endpoint with availability
    [HttpGet]
    public async Task<IActionResult> GetScreenings([FromQuery] int? movieId, [FromQuery] int? theaterId, [FromQuery] int? roomId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var query = _db.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Theater)
            .Include(s => s.Room)
            .Include(s => s.Tickets)
            .Where(s => s.IsActive && s.ShowTime >= DateTime.UtcNow);

        if (movieId.HasValue)
            query = query.Where(s => s.MovieId == movieId.Value);

        if (theaterId.HasValue)
            query = query.Where(s => s.TheaterId == theaterId.Value);

        if (roomId.HasValue)
            query = query.Where(s => s.RoomId == roomId.Value);

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
            RoomId = s.RoomId,
            RoomName = s.Room.Name,
            RoomNumber = s.Room.RoomNumber,
            ShowTime = s.ShowTime,
            Price = s.Price,
            TotalSeats = s.Theater.Rows * s.Theater.SeatsPerRow,
            AvailableSeats = (s.Theater.Rows * s.Theater.SeatsPerRow) - s.Tickets.Count(t => t.Status == "Active"),
            Rows = s.Theater.Rows,
            SeatsPerRow = s.Theater.SeatsPerRow,
            IsActive = s.IsActive,
            ScreeningScheduleId = s.ScreeningScheduleId
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
            .Include(s => s.Room)
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
            RoomId = screening.RoomId,
            RoomName = screening.Room.Name,
            RoomNumber = screening.Room.RoomNumber,
            ShowTime = screening.ShowTime,
            Price = screening.Price,
            TotalSeats = screening.Theater.Rows * screening.Theater.SeatsPerRow,
            AvailableSeats = (screening.Theater.Rows * screening.Theater.SeatsPerRow) - occupiedSeats.Count,
            Rows = screening.Theater.Rows,
            SeatsPerRow = screening.Theater.SeatsPerRow,
            OccupiedSeats = occupiedSeats,
            ScreeningScheduleId = screening.ScreeningScheduleId
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
            .Include(s => s.Room)
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
            RoomId = s.RoomId,
            RoomName = s.Room.Name,
            RoomNumber = s.Room.RoomNumber,
            ShowTime = s.ShowTime,
            Price = s.Price,
            TotalSeats = s.Theater.Rows * s.Theater.SeatsPerRow,
            AvailableSeats = (s.Theater.Rows * s.Theater.SeatsPerRow) - s.Tickets.Count(t => t.Status == "Active"),
            Rows = s.Theater.Rows,
            SeatsPerRow = s.Theater.SeatsPerRow,
            IsActive = s.IsActive,
            ScreeningScheduleId = s.ScreeningScheduleId
        }).ToList();

        return Ok(screeningDtos);
    }

    // GET: api/Screening/room/{roomId}/schedule - Get screenings for a room in a date range (for conflict preview)
    [HttpGet("room/{roomId}/schedule")]
    public async Task<IActionResult> GetRoomSchedule(int roomId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var room = await _db.Rooms.Include(r => r.Theater).FirstOrDefaultAsync(r => r.Id == roomId);
        if (room == null)
            return NotFound("Room not found.");

        var fromDate = from ?? DateTime.UtcNow.Date;
        var toDate = to ?? fromDate.AddDays(30);

        var screenings = await _db.Screenings
            .Include(s => s.Movie)
            .Where(s => s.RoomId == roomId && s.IsActive && s.ShowTime >= fromDate && s.ShowTime <= toDate.AddDays(1))
            .OrderBy(s => s.ShowTime)
            .Select(s => new
            {
                s.Id,
                s.ShowTime,
                EndTime = s.ShowTime.AddMinutes(s.Movie.DurationMinutes),
                MovieTitle = s.Movie.Title,
                DurationMinutes = s.Movie.DurationMinutes,
                s.Price
            })
            .ToListAsync();

        return Ok(new
        {
            RoomId = roomId,
            RoomName = room.Name,
            TheaterName = room.Theater.Name,
            Capacity = room.Theater.Rows * room.Theater.SeatsPerRow,
            FromDate = fromDate,
            ToDate = toDate,
            Screenings = screenings
        });
    }

    // GET: api/Screening/schedules - Get all screening schedules (Admin)
    [HttpGet("schedules")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetScreeningSchedules()
    {
        var schedules = await _db.ScreeningSchedules
            .Include(ss => ss.Movie)
            .Include(ss => ss.Theater)
            .Include(ss => ss.Room)
            .Include(ss => ss.Screenings)
            .Where(ss => ss.IsActive)
            .OrderByDescending(ss => ss.CreatedAt)
            .ToListAsync();

        var scheduleDtos = schedules.Select(ss => new ScreeningScheduleResponseDto
        {
            Id = ss.Id,
            MovieId = ss.MovieId,
            MovieTitle = ss.Movie.Title,
            MoviePosterUrl = EnsureValidImageUrl(ss.Movie.PosterUrl),
            TheaterId = ss.TheaterId,
            TheaterName = ss.Theater.Name,
            RoomId = ss.RoomId,
            RoomName = ss.Room.Name,
            RoomNumber = ss.Room.RoomNumber,
            StartDate = ss.StartDate,
            EndDate = ss.EndDate,
            ShowTimes = JsonSerializer.Deserialize<List<string>>(ss.ShowTimes) ?? new List<string>(),
            DaysOfWeek = ss.DaysOfWeek.Split(',').Select(int.Parse).ToList(),
            Price = ss.Price,
            IsActive = ss.IsActive,
            ScreeningCount = ss.Screenings.Count(s => s.IsActive)
        }).ToList();

        return Ok(scheduleDtos);
    }

    // POST: api/Screening/schedule - Create a screening schedule (batch of screenings) - Admin only
    [HttpPost("schedule")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateScreeningSchedule(ScreeningScheduleDto dto)
    {
        // Validate movie
        var movie = await _db.Movies.FindAsync(dto.MovieId);
        if (movie == null)
            return NotFound("Movie not found.");

        // Validate theater
        var theater = await _db.Theaters.FindAsync(dto.TheaterId);
        if (theater == null)
            return NotFound("Theater not found.");

        if (!theater.IsActive)
            return BadRequest("Theater is not active.");

        // Validate room
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == dto.RoomId && r.TheaterId == dto.TheaterId);
        if (room == null)
            return NotFound("Room not found in the specified theater.");

        if (!room.IsActive)
            return BadRequest("Room is not active.");

        // Validate dates
        if (dto.StartDate.Date < DateTime.UtcNow.Date)
            return BadRequest("Start date must be today or in the future.");

        if (dto.EndDate.Date < dto.StartDate.Date)
            return BadRequest("End date must be after or equal to start date.");

        // Validate show times
        if (dto.ShowTimes == null || dto.ShowTimes.Count == 0)
            return BadRequest("At least one show time is required.");

        foreach (var time in dto.ShowTimes)
        {
            if (!TimeSpan.TryParse(time, out _))
                return BadRequest($"Invalid show time format: {time}. Use HH:mm format.");
        }

        // Validate days of week
        if (dto.DaysOfWeek == null || dto.DaysOfWeek.Count == 0)
            return BadRequest("At least one day of week is required.");

        foreach (var day in dto.DaysOfWeek)
        {
            if (day < 0 || day > 6)
                return BadRequest($"Invalid day of week: {day}. Use 0-6 (Sunday-Saturday).");
        }

        // Create the schedule
        var schedule = new ScreeningSchedule
        {
            MovieId = dto.MovieId,
            TheaterId = dto.TheaterId,
            RoomId = dto.RoomId,
            StartDate = DateTime.SpecifyKind(dto.StartDate.Date, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(dto.EndDate.Date, DateTimeKind.Utc),
            ShowTimes = JsonSerializer.Serialize(dto.ShowTimes),
            DaysOfWeek = string.Join(",", dto.DaysOfWeek),
            Price = dto.Price,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.ScreeningSchedules.Add(schedule);
        await _db.SaveChangesAsync();

        // Generate individual screenings
        var movieDuration = TimeSpan.FromMinutes(movie.DurationMinutes);
        var screeningsCreated = 0;
        var conflicts = new List<string>();

        for (var date = dto.StartDate.Date; date <= dto.EndDate.Date; date = date.AddDays(1))
        {
            var dayOfWeek = (int)date.DayOfWeek;
            if (!dto.DaysOfWeek.Contains(dayOfWeek))
                continue;

            foreach (var timeStr in dto.ShowTimes)
            {
                var time = TimeSpan.Parse(timeStr);
                var showDateTime = DateTime.SpecifyKind(date.Add(time), DateTimeKind.Utc);

                // Skip past show times
                if (showDateTime <= DateTime.UtcNow)
                    continue;

                var screeningEnd = showDateTime.Add(movieDuration);

                // Check for overlapping screenings in the same room
                var existingScreenings = await _db.Screenings
                    .Include(s => s.Movie)
                    .Where(s => s.RoomId == dto.RoomId && s.IsActive && s.ShowTime.Date == date)
                    .ToListAsync();

                var overlapping = existingScreenings.Any(s =>
                {
                    var sEnd = s.ShowTime.Add(TimeSpan.FromMinutes(s.Movie.DurationMinutes));
                    return (showDateTime >= s.ShowTime && showDateTime < sEnd) ||
                           (screeningEnd > s.ShowTime && screeningEnd <= sEnd) ||
                           (showDateTime <= s.ShowTime && screeningEnd >= sEnd);
                });

                if (overlapping)
                {
                    conflicts.Add($"{date:yyyy-MM-dd} {timeStr}");
                    continue;
                }

                var screening = new Screening
                {
                    MovieId = dto.MovieId,
                    TheaterId = dto.TheaterId,
                    RoomId = dto.RoomId,
                    ShowTime = showDateTime,
                    Price = dto.Price,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ScreeningScheduleId = schedule.Id
                };

                _db.Screenings.Add(screening);
                screeningsCreated++;
            }
        }

        await _db.SaveChangesAsync();

        var response = new
        {
            ScheduleId = schedule.Id,
            ScreeningsCreated = screeningsCreated,
            Conflicts = conflicts,
            Message = conflicts.Count > 0 
                ? $"Created {screeningsCreated} screenings. {conflicts.Count} time slots skipped due to conflicts."
                : $"Successfully created {screeningsCreated} screenings."
        };

        return CreatedAtAction(nameof(GetScreening), new { id = schedule.Id }, response);
    }

    // POST: api/Screening - Create a single screening - Admin only
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

        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == dto.RoomId && r.TheaterId == dto.TheaterId);
        if (room == null)
            return NotFound("Room not found in the specified theater.");

        if (!room.IsActive)
            return BadRequest("Room is not active.");

        if (dto.ShowTime <= DateTime.UtcNow)
            return BadRequest("Show time must be in the future.");

        // Check for overlapping screenings in the same room
        var movieDuration = TimeSpan.FromMinutes(movie.DurationMinutes);
        var screeningEnd = dto.ShowTime.Add(movieDuration);
        
        var existingScreenings = await _db.Screenings
            .Include(s => s.Movie)
            .Where(s => s.RoomId == dto.RoomId && s.IsActive)
            .ToListAsync();

        var overlapping = existingScreenings.Any(s =>
        {
            var sEnd = s.ShowTime.Add(TimeSpan.FromMinutes(s.Movie.DurationMinutes));
            return (dto.ShowTime >= s.ShowTime && dto.ShowTime < sEnd) ||
                   (screeningEnd > s.ShowTime && screeningEnd <= sEnd) ||
                   (dto.ShowTime <= s.ShowTime && screeningEnd >= sEnd);
        });

        if (overlapping)
            return BadRequest("There is an overlapping screening in this room.");

        var showTimeUtc = dto.ShowTime.Kind == DateTimeKind.Utc 
            ? dto.ShowTime 
            : DateTime.SpecifyKind(dto.ShowTime, DateTimeKind.Utc);

        var screening = new Screening
        {
            MovieId = dto.MovieId,
            TheaterId = dto.TheaterId,
            RoomId = dto.RoomId,
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
            RoomId = screening.RoomId,
            RoomName = room.Name,
            RoomNumber = room.RoomNumber,
            ShowTime = screening.ShowTime,
            Price = screening.Price,
            TotalSeats = theater.Rows * theater.SeatsPerRow,
            AvailableSeats = theater.Rows * theater.SeatsPerRow,
            Rows = theater.Rows,
            SeatsPerRow = theater.SeatsPerRow,
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
            .Include(s => s.Room)
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

        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.Id == dto.RoomId && r.TheaterId == dto.TheaterId);
        if (room == null)
            return NotFound("Room not found in the specified theater.");

        var showTimeUtc = dto.ShowTime.Kind == DateTimeKind.Utc 
            ? dto.ShowTime 
            : DateTime.SpecifyKind(dto.ShowTime, DateTimeKind.Utc);

        screening.MovieId = dto.MovieId;
        screening.TheaterId = dto.TheaterId;
        screening.RoomId = dto.RoomId;
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
            RoomId = screening.RoomId,
            RoomName = room.Name,
            RoomNumber = room.RoomNumber,
            ShowTime = screening.ShowTime,
            Price = screening.Price,
            TotalSeats = theater.Rows * theater.SeatsPerRow,
            AvailableSeats = theater.Rows * theater.SeatsPerRow,
            Rows = theater.Rows,
            SeatsPerRow = theater.SeatsPerRow,
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

    // DELETE: api/Screening/schedule/{id} - Delete a screening schedule and all its future screenings - Admin only
    [HttpDelete("schedule/{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteScreeningSchedule(int id)
    {
        var schedule = await _db.ScreeningSchedules
            .Include(ss => ss.Screenings)
            .FirstOrDefaultAsync(ss => ss.Id == id);

        if (schedule == null)
            return NotFound("Screening schedule not found.");

        // Deactivate future screenings without tickets
        var futureScreenings = schedule.Screenings
            .Where(s => s.ShowTime > DateTime.UtcNow && s.IsActive)
            .ToList();

        var deactivatedCount = 0;
        var skippedCount = 0;

        foreach (var screening in futureScreenings)
        {
            var hasTickets = await _db.Tickets.AnyAsync(t => t.ScreeningId == screening.Id && t.Status == "Active");
            if (!hasTickets)
            {
                screening.IsActive = false;
                deactivatedCount++;
            }
            else
            {
                skippedCount++;
            }
        }

        schedule.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            Message = $"Schedule deactivated. {deactivatedCount} future screenings deactivated.",
            SkippedScreenings = skippedCount > 0 ? $"{skippedCount} screenings skipped (have active tickets)" : null
        });
    }
}
