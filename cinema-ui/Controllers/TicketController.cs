using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using cinema_ui.Services;
using cinema_ui.Models;

namespace cinema_ui.Controllers;

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

    // Movie Detail Page - Allow anonymous access
    [AllowAnonymous]
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

    // Screening Selection / Seat Selection - Allow anonymous access
    [AllowAnonymous]
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

    // User Information Form - Requires authentication
    [Authorize]
    public async Task<IActionResult> UserInfo(int screeningId, string seatNumbers)
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

        var seatList = seatNumbers?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        var seatCount = seatList.Length;
        var totalPrice = screening.Price * seatCount;

        var viewModel = new UserInfoViewModel
        {
            ScreeningId = screeningId,
            SeatNumbers = seatNumbers ?? "",
            SeatCount = seatCount,
            MovieTitle = movie.Title,
            TheaterName = screening.TheaterName,
            ShowTime = screening.ShowTime,
            PricePerSeat = screening.Price,
            TotalPrice = totalPrice,
            FirstName = profile?.FirstName ?? string.Empty,
            LastName = profile?.LastName ?? string.Empty,
            Email = profile?.Email ?? string.Empty
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize]
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
        TempData["SeatNumbers"] = model.SeatNumbers;
        TempData["PricePerSeat"] = model.PricePerSeat.ToString();
        TempData["TotalPrice"] = model.TotalPrice.ToString();
        TempData["SeatCount"] = model.SeatCount.ToString();
        TempData["MovieTitle"] = model.MovieTitle;

        return RedirectToAction("Payment");
    }

    // Payment Page (Fake) - Requires authentication
    [Authorize]
    public async Task<IActionResult> Payment()
    {
        var screeningId = TempData["ScreeningId"]?.ToString();
        var seatNumbers = TempData["SeatNumbers"]?.ToString();
        var pricePerSeat = TempData["PricePerSeat"]?.ToString();
        var totalPrice = TempData["TotalPrice"]?.ToString();
        var seatCount = TempData["SeatCount"]?.ToString();
        var movieTitle = TempData["MovieTitle"]?.ToString();

        if (string.IsNullOrEmpty(screeningId) || string.IsNullOrEmpty(seatNumbers) || string.IsNullOrEmpty(totalPrice))
        {
            TempData["ErrorMessage"] = "Invalid booking information.";
            return RedirectToAction("Index", "Home");
        }

        var parsedTotalPrice = decimal.Parse(totalPrice);
        var parsedPricePerSeat = decimal.Parse(pricePerSeat ?? totalPrice);
        var parsedSeatCount = int.Parse(seatCount ?? "1");
        var screening = await _apiService.GetScreeningDetailAsync(int.Parse(screeningId));

        var viewModel = new PaymentViewModel
        {
            ScreeningId = int.Parse(screeningId),
            SeatNumbers = seatNumbers,
            SeatCount = parsedSeatCount,
            Price = parsedTotalPrice,
            OriginalPrice = parsedTotalPrice,
            PricePerSeat = parsedPricePerSeat,
            MovieTitle = movieTitle ?? "Unknown Movie",
            TheaterName = screening?.TheaterName,
            ShowTime = screening?.ShowTime
        };

        // Store again for confirmation
        TempData["ScreeningId"] = screeningId;
        TempData["SeatNumbers"] = seatNumbers;

        return View(viewModel);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Payment", model);
        }

        // This is a fake payment - just validate the form, don't actually process payment
        var screeningId = model.ScreeningId;
        var seatNumbers = model.SeatNumbers?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        // Purchase tickets for each seat
        var purchasedTicketIds = new List<int>();
        foreach (var seatNumber in seatNumbers)
        {
            var ticket = await _apiService.PurchaseTicketAsync(screeningId, seatNumber.Trim());
            if (ticket != null)
            {
                purchasedTicketIds.Add(ticket.Id);
            }
        }
        
        if (purchasedTicketIds.Count == 0)
        {
            TempData["ErrorMessage"] = "Failed to purchase tickets. The seats may have been taken or the screening is no longer available.";
            return RedirectToAction("Payment");
        }

        // Store all ticket IDs for confirmation page
        TempData["TicketIds"] = string.Join(",", purchasedTicketIds);
        return RedirectToAction("Confirmation", new { ticketIds = string.Join(",", purchasedTicketIds) });
    }

    // Confirmation Page - Requires authentication
    [Authorize]
    public async Task<IActionResult> Confirmation(string ticketIds)
    {
        var allTickets = await _apiService.GetMyTicketsAsync();
        
        var ticketIdList = ticketIds?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id.Trim(), out var parsed) ? parsed : 0)
            .Where(id => id > 0)
            .ToList() ?? new List<int>();

        var purchasedTickets = allTickets?.Where(t => ticketIdList.Contains(t.Id)).ToList() ?? new List<Ticket>();

        if (purchasedTickets.Count == 0)
        {
            TempData["ErrorMessage"] = "Tickets not found.";
            return RedirectToAction("Tickets", "Profile");
        }

        var profile = await _apiService.GetUserProfileAsync();
        
        var viewModel = new TicketConfirmationViewModel
        {
            Tickets = purchasedTickets,
            UserEmail = profile?.Email ?? "",
            UserName = $"{profile?.FirstName} {profile?.LastName}".Trim()
        };

        return View(viewModel);
    }
}

public class TicketConfirmationViewModel
{
    public List<Ticket> Tickets { get; set; } = new();
    public Ticket? Ticket => Tickets.FirstOrDefault(); // For backwards compatibility
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
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
    public string SeatNumbers { get; set; } = string.Empty;
    public int SeatCount { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string TheaterName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public decimal PricePerSeat { get; set; }
    public decimal TotalPrice { get; set; }

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
    public string SeatNumbers { get; set; } = string.Empty;
    public int SeatCount { get; set; }
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal PricePerSeat { get; set; }
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


