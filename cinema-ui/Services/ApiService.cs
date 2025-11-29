using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using cinema_ui.Models;

namespace cinema_ui.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string ApiKey = "dev-api-key-12345";
    private const string ApiBaseUrl = "http://localhost:5078/api";

    public ApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _httpClient.DefaultRequestHeaders.Add("x-api-key", ApiKey);
    }

    public async Task<LoginResponse?> LoginAsync(string emailOrUsername, string password)
    {
        var request = new
        {
            email = emailOrUsername.Contains("@") ? emailOrUsername : null,
            username = !emailOrUsername.Contains("@") ? emailOrUsername : null,
            password = password
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Auth/login", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        return null;
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        var request = new
        {
            email = email,
            password = password
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Auth/register", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<Movie>?> GetMoviesAsync()
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Movie");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Movie>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        return null;
    }

    public async Task<List<Screening>?> GetScreeningsAsync()
    {
        var response = await _httpClient.GetAsync($"{ApiBaseUrl}/Screening/upcoming?limit=20");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Screening>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        return null;
    }

    public void SetAuthToken(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public void ClearAuthToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public void LoadTokenFromContext()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["authToken"];
        if (!string.IsNullOrEmpty(token))
        {
            SetAuthToken(token);
        }
    }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Director { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string Rating { get; set; } = string.Empty;
}

public class Screening
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int TheaterId { get; set; }
    public string TheaterName { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
}

