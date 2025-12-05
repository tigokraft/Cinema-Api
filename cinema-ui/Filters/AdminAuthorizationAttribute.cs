using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace cinema_ui.Filters;

public class AdminAuthorizationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var token = context.HttpContext.Request.Cookies["authToken"];
        
        if (string.IsNullOrEmpty(token))
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        // Decode JWT token to check role
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            var payload = parts[1];
            // Add padding if needed
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
            var payloadObj = JsonSerializer.Deserialize<JsonElement>(json);

            // Check for role claim
            var role = string.Empty;
            if (payloadObj.TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var roleElement))
            {
                role = roleElement.GetString() ?? string.Empty;
            }
            else if (payloadObj.TryGetProperty("role", out var roleElement2))
            {
                role = roleElement2.GetString() ?? string.Empty;
            }

            if (role != "Admin")
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }
        }
        catch
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        base.OnActionExecuting(context);
    }
}

