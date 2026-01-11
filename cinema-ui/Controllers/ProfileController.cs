using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using cinema_ui.Services;
using cinema_ui.Models;

namespace cinema_ui.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApiService _apiService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(ApiService apiService, ILogger<ProfileController> logger)
    {
        _apiService = apiService;
        _logger = logger;
        _apiService.LoadTokenFromContext();
    }

    public async Task<IActionResult> Index()
    {
        var profile = await _apiService.GetUserProfileAsync();
        if (profile == null)
        {
            TempData["ErrorMessage"] = "Failed to load profile. Please try again.";
            return RedirectToAction("Index", "Home");
        }

        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var profile = await _apiService.GetUserProfileAsync();
            return View("Index", profile);
        }

        var success = await _apiService.UpdateUserProfileAsync(model.FirstName, model.LastName, model.Email);
        if (success)
        {
            TempData["SuccessMessage"] = "Profile updated successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update profile. Please try again.";
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Tickets(string? status)
    {
        var tickets = await _apiService.GetMyTicketsAsync(status);
        var viewModel = new MyTicketsViewModel
        {
            Tickets = tickets ?? new List<Ticket>(),
            SelectedStatus = status
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CancelTicket(int ticketId)
    {
        var (success, message) = await _apiService.CancelTicketAsync(ticketId);
        
        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }
        
        return RedirectToAction("Tickets");
    }
}

public class UpdateProfileViewModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}

public class MyTicketsViewModel
{
    public List<Ticket> Tickets { get; set; } = new();
    public string? SelectedStatus { get; set; }
}

