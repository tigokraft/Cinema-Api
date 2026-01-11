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

        model.ErrorMessage = "Invalid email/username or password. If you just registered, please verify your email first.";
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

        var response = await _apiService.RegisterAsync(model.Email, model.Password);

        if (response != null && response.RequiresVerification)
        {
            // Store verification data in TempData for the verification page
            TempData["VerifyUserId"] = response.UserId;
            TempData["VerifyEmail"] = response.Email;
            TempData["VerifyCode"] = response.VerificationCode;
            TempData["VerifyUsername"] = response.Username;
            
            return RedirectToAction("VerifyEmail");
        }

        model.ErrorMessage = "Registration failed. Email may already be in use.";
        return View(model);
    }

    [HttpGet]
    public IActionResult VerifyEmail()
    {
        var userId = TempData["VerifyUserId"];
        var email = TempData["VerifyEmail"] as string;
        var code = TempData["VerifyCode"] as string;
        var username = TempData["VerifyUsername"] as string;
        
        if (userId == null || email == null)
        {
            return RedirectToAction("Register");
        }
        
        // Keep the data available for the view and potential resend
        TempData.Keep("VerifyUserId");
        TempData.Keep("VerifyEmail");
        TempData.Keep("VerifyCode");
        TempData.Keep("VerifyUsername");
        
        var viewModel = new VerifyEmailViewModel
        {
            UserId = (int)userId,
            Email = email,
            VerificationCode = code ?? "",
            Username = username ?? ""
        };
        
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
    {
        var (success, message) = await _apiService.VerifyEmailAsync(model.UserId, model.EnteredCode);
        
        if (success)
        {
            TempData["SuccessMessage"] = "Email verified successfully! You can now log in.";
            return RedirectToAction("Login");
        }
        
        model.ErrorMessage = message;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResendVerification(int userId)
    {
        var response = await _apiService.ResendVerificationAsync(userId);
        
        if (response != null)
        {
            TempData["VerifyUserId"] = userId;
            TempData["VerifyEmail"] = response.Email;
            TempData["VerifyCode"] = response.VerificationCode;
            TempData["ResendMessage"] = "New verification code sent!";
        }
        
        return RedirectToAction("VerifyEmail");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("authToken");
        _apiService.ClearAuthToken();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var response = await _apiService.ForgotPasswordAsync(model.Email);

        if (response != null && response.UserId > 0)
        {
            TempData["ResetUserId"] = response.UserId;
            TempData["ResetEmail"] = response.Email;
            TempData["ResetCode"] = response.ResetCode;
            return RedirectToAction("ResetPassword");
        }

        // Even if email doesn't exist, show success for security
        model.SuccessMessage = "If this email exists, a reset code has been sent.";
        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPassword()
    {
        var userId = TempData["ResetUserId"];
        var email = TempData["ResetEmail"] as string;
        var code = TempData["ResetCode"] as string;
        
        if (userId == null || email == null)
        {
            return RedirectToAction("ForgotPassword");
        }
        
        TempData.Keep("ResetUserId");
        TempData.Keep("ResetEmail");
        TempData.Keep("ResetCode");
        
        return View(new ResetPasswordViewModel
        {
            UserId = (int)userId,
            Email = email,
            ResetCode = code ?? ""
        });
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (model.NewPassword != model.ConfirmPassword)
        {
            model.ErrorMessage = "Passwords do not match.";
            return View(model);
        }

        var (success, message) = await _apiService.ResetPasswordAsync(model.UserId, model.EnteredCode, model.NewPassword);
        
        if (success)
        {
            TempData["SuccessMessage"] = "Password reset successfully! You can now sign in.";
            return RedirectToAction("Login");
        }
        
        model.ErrorMessage = message;
        return View(model);
    }
}

public class VerifyEmailViewModel
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string VerificationCode { get; set; } = string.Empty;
    public string EnteredCode { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class ForgotPasswordViewModel
{
    public string Email { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
}

public class ResetPasswordViewModel
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string ResetCode { get; set; } = string.Empty;
    public string EnteredCode { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

