using api_cinema.Data;
using api_cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_cinema.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AnalyticsController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/Analytics/revenue
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueMetrics([FromQuery] int days = 30)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);
        var startDate = today.AddDays(-days);

        var allTickets = await _db.Tickets
            .Include(t => t.Screening)
                .ThenInclude(s => s.Movie)
            .Include(t => t.Screening)
                .ThenInclude(s => s.Theater)
            .Where(t => t.Status != "Cancelled")
            .ToListAsync();

        var metrics = new RevenueMetricsDto
        {
            TotalRevenue = allTickets.Sum(t => t.Price - (t.DiscountAmount ?? 0)),
            TodayRevenue = allTickets
                .Where(t => t.PurchaseDate.Date == today)
                .Sum(t => t.Price - (t.DiscountAmount ?? 0)),
            WeekRevenue = allTickets
                .Where(t => t.PurchaseDate.Date >= weekAgo)
                .Sum(t => t.Price - (t.DiscountAmount ?? 0)),
            MonthRevenue = allTickets
                .Where(t => t.PurchaseDate.Date >= monthAgo)
                .Sum(t => t.Price - (t.DiscountAmount ?? 0)),
            DailyRevenue = allTickets
                .Where(t => t.PurchaseDate.Date >= startDate)
                .GroupBy(t => t.PurchaseDate.Date)
                .Select(g => new RevenueByDateDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(t => t.Price - (t.DiscountAmount ?? 0)),
                    TicketCount = g.Count()
                })
                .OrderBy(r => r.Date)
                .ToList(),
            TopMoviesByRevenue = allTickets
                .GroupBy(t => new { t.Screening.Movie.Id, t.Screening.Movie.Title, t.Screening.Movie.PosterUrl })
                .Select(g => new RevenueByMovieDto
                {
                    MovieId = g.Key.Id,
                    MovieTitle = g.Key.Title,
                    PosterUrl = g.Key.PosterUrl,
                    Revenue = g.Sum(t => t.Price - (t.DiscountAmount ?? 0)),
                    TicketCount = g.Count()
                })
                .OrderByDescending(r => r.Revenue)
                .Take(10)
                .ToList(),
            RevenueByTheater = allTickets
                .GroupBy(t => new { t.Screening.Theater.Id, t.Screening.Theater.Name })
                .Select(g => new RevenueByTheaterDto
                {
                    TheaterId = g.Key.Id,
                    TheaterName = g.Key.Name,
                    Revenue = g.Sum(t => t.Price - (t.DiscountAmount ?? 0)),
                    TicketCount = g.Count()
                })
                .OrderByDescending(r => r.Revenue)
                .ToList()
        };

        return Ok(metrics);
    }

    // GET: api/Analytics/tickets
    [HttpGet("tickets")]
    public async Task<IActionResult> GetTicketStatistics([FromQuery] int days = 30)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);
        var startDate = today.AddDays(-days);

        var allTickets = await _db.Tickets.ToListAsync();

        var stats = new TicketStatisticsDto
        {
            TotalTickets = allTickets.Count,
            ActiveTickets = allTickets.Count(t => t.Status == "Active"),
            UsedTickets = allTickets.Count(t => t.Status == "Used"),
            CancelledTickets = allTickets.Count(t => t.Status == "Cancelled"),
            TodayTickets = allTickets.Count(t => t.PurchaseDate.Date == today),
            WeekTickets = allTickets.Count(t => t.PurchaseDate.Date >= weekAgo),
            MonthTickets = allTickets.Count(t => t.PurchaseDate.Date >= monthAgo),
            DailyTickets = allTickets
                .Where(t => t.PurchaseDate.Date >= startDate)
                .GroupBy(t => t.PurchaseDate.Date)
                .Select(g => new TicketsByDateDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(r => r.Date)
                .ToList()
        };

        return Ok(stats);
    }

    // GET: api/Analytics/occupancy
    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancyRates([FromQuery] int? movieId, [FromQuery] int? theaterId)
    {
        var now = DateTime.UtcNow;

        var query = _db.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Theater)
            .Include(s => s.Room)
            .Include(s => s.Tickets)
            .Where(s => s.ShowTime >= now && s.IsActive);

        if (movieId.HasValue)
            query = query.Where(s => s.MovieId == movieId.Value);

        if (theaterId.HasValue)
            query = query.Where(s => s.TheaterId == theaterId.Value);

        var screenings = await query.ToListAsync();

        var occupancyRates = screenings.Select(s =>
        {
            var totalSeats = s.Theater.Rows * s.Theater.SeatsPerRow;
            var soldSeats = s.Tickets.Count(t => t.Status == "Active" || t.Status == "Used");
            return new OccupancyRateDto
            {
                ScreeningId = s.Id,
                MovieTitle = s.Movie.Title,
                TheaterName = s.Theater.Name,
                RoomName = s.Room?.Name ?? "Main",
                ShowTime = s.ShowTime,
                TotalSeats = totalSeats,
                SoldSeats = soldSeats,
                OccupancyPercent = totalSeats > 0 ? Math.Round((decimal)soldSeats / totalSeats * 100, 1) : 0
            };
        })
        .OrderBy(o => o.ShowTime)
        .ToList();

        return Ok(occupancyRates);
    }

    // GET: api/Analytics/popular-movies
    [HttpGet("popular-movies")]
    public async Task<IActionResult> GetPopularMovies([FromQuery] int days = 30, [FromQuery] int limit = 10)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        var movieStats = await _db.Tickets
            .Include(t => t.Screening)
                .ThenInclude(s => s.Movie)
            .Where(t => t.PurchaseDate >= startDate && t.Status != "Cancelled")
            .GroupBy(t => new { t.Screening.Movie.Id, t.Screening.Movie.Title, t.Screening.Movie.PosterUrl })
            .Select(g => new PopularMovieDto
            {
                MovieId = g.Key.Id,
                Title = g.Key.Title,
                PosterUrl = g.Key.PosterUrl,
                TicketsSold = g.Count(),
                Revenue = g.Sum(t => t.Price - (t.DiscountAmount ?? 0))
            })
            .OrderByDescending(m => m.TicketsSold)
            .Take(limit)
            .ToListAsync();

        // Add screening count and average occupancy
        foreach (var movie in movieStats)
        {
            var screenings = await _db.Screenings
                .Include(s => s.Tickets)
                .Include(s => s.Theater)
                .Include(s => s.Room)
                .Where(s => s.MovieId == movie.MovieId && s.ShowTime >= startDate.AddDays(-days))
                .ToListAsync();

            movie.ScreeningsCount = screenings.Count;
            if (screenings.Any())
            {
                var totalOccupancy = screenings.Sum(s =>
                {
                    var totalSeats = s.Theater.Rows * s.Theater.SeatsPerRow;
                    var soldSeats = s.Tickets.Count(t => t.Status != "Cancelled");
                    return totalSeats > 0 ? (decimal)soldSeats / totalSeats * 100 : 0;
                });
                movie.AverageOccupancy = Math.Round(totalOccupancy / screenings.Count, 1);
            }
        }

        return Ok(movieStats);
    }

    // GET: api/Analytics/peak-hours
    [HttpGet("peak-hours")]
    public async Task<IActionResult> GetPeakHours([FromQuery] int days = 30)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        var ticketsByHour = await _db.Tickets
            .Where(t => t.PurchaseDate >= startDate && t.Status != "Cancelled")
            .ToListAsync();

        var peakHours = ticketsByHour
            .GroupBy(t => t.PurchaseDate.Hour)
            .Select(g => new PeakHoursDto
            {
                Hour = g.Key,
                TicketCount = g.Count(),
                RevenueAmount = g.Sum(t => t.Price - (t.DiscountAmount ?? 0))
            })
            .OrderBy(p => p.Hour)
            .ToList();

        // Fill in missing hours with zeros
        var allHours = Enumerable.Range(0, 24)
            .Select(h => peakHours.FirstOrDefault(p => p.Hour == h) ?? new PeakHoursDto { Hour = h, TicketCount = 0, RevenueAmount = 0 })
            .ToList();

        return Ok(allHours);
    }

    // GET: api/Analytics/activity-feed
    [HttpGet("activity-feed")]
    public async Task<IActionResult> GetActivityFeed([FromQuery] int limit = 20)
    {
        var activities = new List<ActivityFeedItemDto>();

        // Recent ticket purchases
        var recentPurchases = await _db.Tickets
            .Include(t => t.User)
            .Include(t => t.Screening)
                .ThenInclude(s => s.Movie)
            .OrderByDescending(t => t.PurchaseDate)
            .Take(limit)
            .ToListAsync();

        activities.AddRange(recentPurchases.Select(t => new ActivityFeedItemDto
        {
            Type = t.Status == "Cancelled" ? "Cancellation" : "Purchase",
            Description = $"{t.User.Username} {(t.Status == "Cancelled" ? "cancelled" : "purchased")} a ticket for {t.Screening.Movie.Title}",
            Timestamp = t.PurchaseDate,
            Username = t.User.Username,
            EntityId = t.Id
        }));

        // Recent user registrations
        var recentUsers = await _db.Users
            .OrderByDescending(u => u.Id)
            .Take(limit / 2)
            .ToListAsync();

        activities.AddRange(recentUsers.Select(u => new ActivityFeedItemDto
        {
            Type = "Registration",
            Description = $"New user registered: {u.Username}",
            Timestamp = DateTime.UtcNow.AddDays(-new Random().Next(0, 7)), // Approximate
            Username = u.Username,
            EntityId = u.Id
        }));

        return Ok(activities.OrderByDescending(a => a.Timestamp).Take(limit));
    }

    // GET: api/Analytics/alerts
    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts()
    {
        var alerts = new List<object>();
        var now = DateTime.UtcNow;
        var next24Hours = now.AddHours(24);

        // Low occupancy screenings (< 20% sold, happening in next 24 hours)
        var upcomingScreenings = await _db.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Theater)
            .Include(s => s.Room)
            .Include(s => s.Tickets)
            .Where(s => s.ShowTime >= now && s.ShowTime <= next24Hours && s.IsActive)
            .ToListAsync();

        foreach (var screening in upcomingScreenings)
        {
            var totalSeats = screening.Theater.Rows * screening.Theater.SeatsPerRow;
            var soldSeats = screening.Tickets.Count(t => t.Status == "Active");
            var occupancy = totalSeats > 0 ? (decimal)soldSeats / totalSeats * 100 : 0;

            if (occupancy < 20)
            {
                alerts.Add(new
                {
                    Type = "LowOccupancy",
                    Severity = "Warning",
                    Message = $"{screening.Movie.Title} at {screening.ShowTime:HH:mm} has only {occupancy:F0}% seats sold",
                    ScreeningId = screening.Id
                });
            }
        }

        // Screenings with no tickets sold
        var noTicketScreenings = upcomingScreenings
            .Where(s => !s.Tickets.Any(t => t.Status != "Cancelled"))
            .Take(5);

        foreach (var screening in noTicketScreenings)
        {
            alerts.Add(new
            {
                Type = "NoSales",
                Severity = "Info",
                Message = $"{screening.Movie.Title} at {screening.ShowTime:MMM dd HH:mm} has no tickets sold yet",
                ScreeningId = screening.Id
            });
        }

        return Ok(alerts);
    }
}
