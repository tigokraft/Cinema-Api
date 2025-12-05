using Microsoft.AspNetCore.Mvc;
using cinema_ui.Models;
using cinema_ui.Services;

namespace cinema_ui.Controllers;

public class AuthController : Controller
{
    private readonly ApiService _apiService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApiService apiService, ILogger<AuthController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var response = await _apiService.LoginAsync(model.EmailOrUsername, model.Password);

        if (response != null && !string.IsNullOrEmpty(response.Token))
        {
            // Store token in cookie
            Response.Cookies.Append("authToken", response.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to true in production with HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(1)
            });

            // Set token in API service for subsequent requests
            _apiService.SetAuthToken(response.Token);

            return RedirectToAction("Index", "Home");
        }

        model.ErrorMessage = "Invalid email/username or password.";
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Password != model.ConfirmPassword)
        {
            model.ErrorMessage = "Passwords do not match.";
            return View(model);
        }

        var success = await _apiService.RegisterAsync(model.Email, model.Password);

        if (success)
        {
            return RedirectToAction("Login", "Auth");
        }

        model.ErrorMessage = "Registration failed. Email may already be in use.";
        return View(model);
    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("authToken");
        _apiService.ClearAuthToken();
        return RedirectToAction("Index", "Home");
    }
}

