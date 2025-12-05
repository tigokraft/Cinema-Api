using System.ComponentModel.DataAnnotations;

namespace cinema_ui.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email or username is required")]
    [Display(Name = "Email or Username")]
    public string EmailOrUsername { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    public string? ErrorMessage { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Please confirm your password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    public string? ErrorMessage { get; set; }
}

