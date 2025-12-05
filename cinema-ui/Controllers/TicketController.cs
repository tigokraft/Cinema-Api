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
    public async Task<IActionResult> MovieDetail(int id)
    {
        var movie = await _apiService.GetMovieAsync(id);
        if (movie == null)
        {
            TempData["ErrorMessage"] = "Movie not found.";
            return RedirectToAction("Index", "Home");
        }

        var screenings = await _apiService.GetScreeningsByMovieAsync(id);
        var viewModel = new MovieDetailViewModel
        {
            Movie = movie,
            Screenings = screenings ?? new List<Screening>()
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
    public IActionResult Payment()
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

        var viewModel = new PaymentViewModel
        {
            ScreeningId = int.Parse(screeningId),
            SeatNumber = seatNumber,
            Price = decimal.Parse(price),
            MovieTitle = movieTitle ?? "Unknown Movie"
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
    public string MovieTitle { get; set; } = string.Empty;

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

