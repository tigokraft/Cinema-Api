using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace cinema_ui.Filters;

public class JwtCookieAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public JwtCookieAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Get JWT token from cookie
        var token = Request.Cookies["authToken"];
        
        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        try
        {
            // Decode JWT token to extract claims
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
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

            // Extract claims
            var claims = new List<Claim>();
            
            // Extract user ID
            if (payloadObj.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out var userIdElement))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userIdElement.GetString() ?? ""));
            }
            else if (payloadObj.TryGetProperty("nameid", out var userIdElement2))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userIdElement2.GetString() ?? ""));
            }

            // Extract username
            if (payloadObj.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", out var usernameElement))
            {
                claims.Add(new Claim(ClaimTypes.Name, usernameElement.GetString() ?? ""));
            }
            else if (payloadObj.TryGetProperty("unique_name", out var usernameElement2))
            {
                claims.Add(new Claim(ClaimTypes.Name, usernameElement2.GetString() ?? ""));
            }

            // Extract role
            if (payloadObj.TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var roleElement))
            {
                var role = roleElement.GetString();
                if (!string.IsNullOrEmpty(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            else if (payloadObj.TryGetProperty("role", out var roleElement2))
            {
                var role = roleElement2.GetString();
                if (!string.IsNullOrEmpty(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
    }
}

