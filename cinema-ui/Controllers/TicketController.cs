using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using cinema_ui.Services;
using cinema_ui.Models;

namespace cinema_ui.Controllers;

[Authorize]
public class TicketController : Controller
{
    private readonly ApiService _apiService;
    private readonly ILogger<TicketController> _logger;

    public TicketController(ApiService apiService, ILogger<TicketController> logger)
    {
        _apiService = apiService;
        _logger = logger;
        _apiService.LoadTokenFromContext();
    }

    // Movie Detail Page
    public async Task<IActionResult> MovieDetail(int id, DateTime? date = null, int? theaterId = null)
    {
        var movie = await _apiService.GetMovieAsync(id);
        if (movie == null)
        {
            TempData["ErrorMessage"] = "Movie not found.";
            return RedirectToAction("Index", "Home");
        }

        var allScreenings = await _apiService.GetScreeningsByMovieAsync(id);
        var screenings = allScreenings ?? new List<Screening>();
        
        // Get unique dates and theaters for filter options
        var availableDates = screenings
            .Select(s => s.ShowTime.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();
        
        var availableTheaters = screenings
            .Select(s => new { s.TheaterId, s.TheaterName })
            .DistinctBy(t => t.TheaterId)
            .OrderBy(t => t.TheaterName)
            .ToList();
        
        // Apply filters
        if (date.HasValue)
        {
            screenings = screenings.Where(s => s.ShowTime.Date == date.Value.Date).ToList();
        }
        
        if (theaterId.HasValue)
        {
            screenings = screenings.Where(s => s.TheaterId == theaterId.Value).ToList();
        }
        
        // Group screenings by date
        var groupedScreenings = screenings
            .GroupBy(s => s.ShowTime.Date)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.OrderBy(s => s.ShowTime).ToList());
        
        var viewModel = new MovieDetailViewModel
        {
            Movie = movie,
            Screenings = screenings,
            GroupedScreenings = groupedScreenings,
            AvailableDates = availableDates,
            AvailableTheaters = availableTheaters.Select(t => new TheaterOption { Id = t.TheaterId, Name = t.TheaterName }).ToList(),
            SelectedDate = date,
            SelectedTheaterId = theaterId
        };

        return View(viewModel);
    }

    // Screening Selection / Seat Selection
    public async Task<IActionResult> SelectSeats(int screeningId)
    {
        var screening = await _apiService.GetScreeningDetailAsync(screeningId);
        if (screening == null)
        {
            TempData["ErrorMessage"] = "Screening not found.";
            return RedirectToAction("Index", "Home");
        }

        var movie = await _apiService.GetMovieAsync(screening.MovieId);
        if (movie == null)
        {
            TempData["ErrorMessage"] = "Movie not found.";
            return RedirectToAction("Index", "Home");
        }

        var viewModel = new SeatSelectionViewModel
        {
            Screening = screening,
            Movie = movie
        };

        return View(viewModel);
    }

    // User Information Form
    public async Task<IActionResult> UserInfo(int screeningId, string seatNumber)
    {
        var profile = await _apiService.GetUserProfileAsync();
        var screening = await _apiService.GetScreeningDetailAsync(screeningId);
        if (screening == null)
        {
            TempData["ErrorMessage"] = "Screening not found.";
            return RedirectToAction("Index", "Home");
        }

        var movie = await _apiService.GetMovieAsync(screening.MovieId);
        if (movie == null)
        {
            TempData["ErrorMessage"] = "Movie not found.";
            return RedirectToAction("Index", "Home");
        }

        var viewModel = new UserInfoViewModel
        {
            ScreeningId = screeningId,
            SeatNumber = seatNumber,
            MovieTitle = movie.Title,
            TheaterName = screening.TheaterName,
            ShowTime = screening.ShowTime,
            Price = screening.Price,
            FirstName = profile?.FirstName ?? string.Empty,
            LastName = profile?.LastName ?? string.Empty,
            Email = profile?.Email ?? string.Empty
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UserInfo(UserInfoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Update user profile first
        await _apiService.UpdateUserProfileAsync(model.FirstName, model.LastName, model.Email);

        // Store in TempData for payment page
        TempData["ScreeningId"] = model.ScreeningId;
        TempData["SeatNumber"] = model.SeatNumber;
        TempData["Price"] = model.Price.ToString();
        TempData["MovieTitle"] = model.MovieTitle;

        return RedirectToAction("Payment");
    }

    // Payment Page (Fake)
    public async Task<IActionResult> Payment()
    {
        var screeningId = TempData["ScreeningId"]?.ToString();
        var seatNumber = TempData["SeatNumber"]?.ToString();
        var price = TempData["Price"]?.ToString();
        var movieTitle = TempData["MovieTitle"]?.ToString();

        if (string.IsNullOrEmpty(screeningId) || string.IsNullOrEmpty(seatNumber) || string.IsNullOrEmpty(price))
        {
            TempData["ErrorMessage"] = "Invalid booking information.";
            return RedirectToAction("Index", "Home");
        }

        var parsedPrice = decimal.Parse(price);
        var screening = await _apiService.GetScreeningDetailAsync(int.Parse(screeningId));

        var viewModel = new PaymentViewModel
        {
            ScreeningId = int.Parse(screeningId),
            SeatNumber = seatNumber,
            Price = parsedPrice,
            OriginalPrice = parsedPrice,
            MovieTitle = movieTitle ?? "Unknown Movie",
            TheaterName = screening?.TheaterName,
            ShowTime = screening?.ShowTime
        };

        // Store again for confirmation
        TempData["ScreeningId"] = screeningId;
        TempData["SeatNumber"] = seatNumber;

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Payment", model);
        }

        // This is a fake payment - just validate the form, don't actually process payment
        var screeningId = model.ScreeningId;
        var seatNumber = model.SeatNumber;

        // Purchase the ticket
        var ticket = await _apiService.PurchaseTicketAsync(screeningId, seatNumber);
        
        if (ticket == null)
        {
            TempData["ErrorMessage"] = "Failed to purchase ticket. The seat may have been taken or the screening is no longer available.";
            return RedirectToAction("Payment");
        }

        TempData["TicketId"] = ticket.Id;
        return RedirectToAction("Confirmation", new { ticketId = ticket.Id });
    }

    // Confirmation Page
    public async Task<IActionResult> Confirmation(int ticketId)
    {
        var tickets = await _apiService.GetMyTicketsAsync();
        var ticket = tickets?.FirstOrDefault(t => t.Id == ticketId);

        if (ticket == null)
        {
            TempData["ErrorMessage"] = "Ticket not found.";
            return RedirectToAction("Tickets", "Profile");
        }

        return View(ticket);
    }
}

public class MovieDetailViewModel
{
    public Movie Movie { get; set; } = null!;
    public List<Screening> Screenings { get; set; } = new();
    public Dictionary<DateTime, List<Screening>> GroupedScreenings { get; set; } = new();
    public List<DateTime> AvailableDates { get; set; } = new();
    public List<TheaterOption> AvailableTheaters { get; set; } = new();
    public DateTime? SelectedDate { get; set; }
    public int? SelectedTheaterId { get; set; }
}

public class TheaterOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SeatSelectionViewModel
{
    public ScreeningDetail Screening { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}

public class UserInfoViewModel
{
    public int ScreeningId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public string TheaterName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public decimal Price { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class PaymentViewModel
{
    public int ScreeningId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string? TheaterName { get; set; }
    public DateTime? ShowTime { get; set; }

    [Display(Name = "Promo Code")]
    public string? PromoCode { get; set; }

    [Required]
    [Display(Name = "Card Number")]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Card Holder Name")]
    public string CardHolderName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Expiry Date")]
    public string ExpiryDate { get; set; } = string.Empty;

    [Required]
    [Display(Name = "CVV")]
    public string Cvv { get; set; } = string.Empty;
}

