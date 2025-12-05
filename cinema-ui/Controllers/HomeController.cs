using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using cinema_ui.Models;
using cinema_ui.Services;

namespace cinema_ui.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApiService _apiService;

    public HomeController(ILogger<HomeController> logger, ApiService apiService)
    {
        _logger = logger;
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        // Load auth token from cookie if exists
        _apiService.LoadTokenFromContext();

        var movies = await _apiService.GetMoviesAsync();
        var screenings = await _apiService.GetScreeningsAsync();
        var featuredMovie = await _apiService.GetFeaturedMovieAsync();

        var viewModel = new HomeViewModel
        {
            Movies = movies ?? new List<cinema_ui.Services.Movie>(),
            Screenings = screenings ?? new List<cinema_ui.Services.Screening>(),
            FeaturedMovie = featuredMovie
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
