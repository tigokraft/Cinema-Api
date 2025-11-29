using Microsoft.AspNetCore.Mvc;
using cinema_ui.Filters;
using cinema_ui.Services;
using cinema_ui.Models;

namespace cinema_ui.Controllers;

[AdminAuthorization]
public class AdminController : Controller
{
    private readonly AdminApiService _adminApi;
    private readonly ILogger<AdminController> _logger;

    public AdminController(AdminApiService adminApi, ILogger<AdminController> logger)
    {
        _adminApi = adminApi;
        _logger = logger;
    }

    // Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var movies = await _adminApi.GetAllMoviesAsync() ?? new List<MovieResponseDto>();
        var theaters = await _adminApi.GetAllTheatersAsync() ?? new List<TheaterResponseDto>();
        var screenings = await _adminApi.GetAllScreeningsAsync() ?? new List<ScreeningResponseDto>();
        var users = await _adminApi.GetAllUsersAsync() ?? new List<UserResponseDto>();

        var viewModel = new AdminDashboardViewModel
        {
            TotalMovies = movies.Count,
            ActiveMovies = movies.Count(m => m.IsActive),
            TotalTheaters = theaters.Count,
            ActiveTheaters = theaters.Count(t => t.IsActive),
            TotalScreenings = screenings.Count,
            ActiveScreenings = screenings.Count(s => s.IsActive),
            TotalUsers = users.Count,
            AdminUsers = users.Count(u => u.Role == "Admin")
        };

        return View(viewModel);
    }

    // Movies Management
    public async Task<IActionResult> Movies()
    {
        var movies = await _adminApi.GetAllMoviesAsync(includeInactive: true) ?? new List<MovieResponseDto>();
        return View(movies);
    }

    [HttpGet]
    public IActionResult CreateMovie()
    {
        return View(new AdminMovieViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreateMovie(AdminMovieViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = new MovieDto
        {
            Title = model.Title,
            Description = model.Description,
            Genre = model.Genre,
            DurationMinutes = model.DurationMinutes,
            ReleaseDate = model.ReleaseDate,
            Director = model.Director,
            PosterUrl = model.PosterUrl,
            Rating = model.Rating
        };

        var result = await _adminApi.CreateMovieAsync(dto);
        if (result != null)
        {
            return RedirectToAction("Movies");
        }

        model.ErrorMessage = "Failed to create movie.";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditMovie(int id)
    {
        var movie = await _adminApi.GetMovieAsync(id);
        if (movie == null)
            return RedirectToAction("Movies");

        var model = new AdminMovieViewModel
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            Genre = movie.Genre,
            DurationMinutes = movie.DurationMinutes,
            ReleaseDate = movie.ReleaseDate,
            Director = movie.Director,
            PosterUrl = movie.PosterUrl,
            Rating = movie.Rating,
            IsActive = movie.IsActive
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditMovie(AdminMovieViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = new MovieDto
        {
            Title = model.Title,
            Description = model.Description,
            Genre = model.Genre,
            DurationMinutes = model.DurationMinutes,
            ReleaseDate = model.ReleaseDate,
            Director = model.Director,
            PosterUrl = model.PosterUrl,
            Rating = model.Rating
        };

        var result = await _adminApi.UpdateMovieAsync(model.Id, dto);
        if (result != null)
        {
            return RedirectToAction("Movies");
        }

        model.ErrorMessage = "Failed to update movie.";
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMovie(int id)
    {
        var success = await _adminApi.DeleteMovieAsync(id);
        return RedirectToAction("Movies");
    }

    // Theaters Management
    public async Task<IActionResult> Theaters()
    {
        var theaters = await _adminApi.GetAllTheatersAsync(includeInactive: true) ?? new List<TheaterResponseDto>();
        return View(theaters);
    }

    [HttpGet]
    public IActionResult CreateTheater()
    {
        return View(new AdminTheaterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreateTheater(AdminTheaterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.Capacity != model.Rows * model.SeatsPerRow)
        {
            model.ErrorMessage = "Capacity must equal Rows × SeatsPerRow.";
            return View(model);
        }

        var dto = new TheaterDto
        {
            Name = model.Name,
            Capacity = model.Capacity,
            Rows = model.Rows,
            SeatsPerRow = model.SeatsPerRow
        };

        var result = await _adminApi.CreateTheaterAsync(dto);
        if (result != null)
        {
            return RedirectToAction("Theaters");
        }

        model.ErrorMessage = "Failed to create theater.";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditTheater(int id)
    {
        var theater = await _adminApi.GetTheaterAsync(id);
        
        if (theater == null)
            return RedirectToAction("Theaters");

        var model = new AdminTheaterViewModel
        {
            Id = theater.Id,
            Name = theater.Name,
            Capacity = theater.Capacity,
            Rows = theater.Rows,
            SeatsPerRow = theater.SeatsPerRow,
            IsActive = theater.IsActive
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditTheater(AdminTheaterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.Capacity != model.Rows * model.SeatsPerRow)
        {
            model.ErrorMessage = "Capacity must equal Rows × SeatsPerRow.";
            return View(model);
        }

        var dto = new TheaterDto
        {
            Name = model.Name,
            Capacity = model.Capacity,
            Rows = model.Rows,
            SeatsPerRow = model.SeatsPerRow
        };

        var result = await _adminApi.UpdateTheaterAsync(model.Id, dto);
        if (result != null)
        {
            return RedirectToAction("Theaters");
        }

        model.ErrorMessage = "Failed to update theater.";
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTheater(int id)
    {
        await _adminApi.DeleteTheaterAsync(id);
        return RedirectToAction("Theaters");
    }

    // Screenings Management
    public async Task<IActionResult> Screenings()
    {
        var screenings = await _adminApi.GetAllScreeningsAsync() ?? new List<ScreeningResponseDto>();
        var movies = await _adminApi.GetAllMoviesAsync() ?? new List<MovieResponseDto>();
        var theaters = await _adminApi.GetAllTheatersAsync() ?? new List<TheaterResponseDto>();

        var viewModel = new AdminScreeningsViewModel
        {
            Screenings = screenings,
            Movies = movies,
            Theaters = theaters
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> CreateScreening()
    {
        var movies = await _adminApi.GetAllMoviesAsync() ?? new List<MovieResponseDto>();
        var theaters = await _adminApi.GetAllTheatersAsync() ?? new List<TheaterResponseDto>();

        var model = new AdminScreeningViewModel
        {
            AvailableMovies = movies.Where(m => m.IsActive).ToList(),
            AvailableTheaters = theaters.Where(t => t.IsActive).ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateScreening(AdminScreeningViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var movies = await _adminApi.GetAllMoviesAsync() ?? new List<MovieResponseDto>();
            var theaters = await _adminApi.GetAllTheatersAsync() ?? new List<TheaterResponseDto>();
            model.AvailableMovies = movies.Where(m => m.IsActive).ToList();
            model.AvailableTheaters = theaters.Where(t => t.IsActive).ToList();
            return View(model);
        }

        var dto = new ScreeningDto
        {
            MovieId = model.MovieId,
            TheaterId = model.TheaterId,
            ShowTime = model.ShowTime,
            Price = model.Price
        };

        var result = await _adminApi.CreateScreeningAsync(dto);
        if (result != null)
        {
            return RedirectToAction("Screenings");
        }

        model.ErrorMessage = "Failed to create screening. Check for overlapping times or inactive theater/movie.";
        var moviesRetry = await _adminApi.GetAllMoviesAsync() ?? new List<MovieResponseDto>();
        var theatersRetry = await _adminApi.GetAllTheatersAsync() ?? new List<TheaterResponseDto>();
        model.AvailableMovies = moviesRetry.Where(m => m.IsActive).ToList();
        model.AvailableTheaters = theatersRetry.Where(t => t.IsActive).ToList();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteScreening(int id)
    {
        await _adminApi.DeleteScreeningAsync(id);
        return RedirectToAction("Screenings");
    }

    // Users Management
    public async Task<IActionResult> Users(string? role, string? search)
    {
        var users = await _adminApi.GetAllUsersAsync(role, search) ?? new List<UserResponseDto>();
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateUserRole(int id, string role)
    {
        var success = await _adminApi.UpdateUserRoleAsync(id, role);
        return RedirectToAction("Users");
    }
}

