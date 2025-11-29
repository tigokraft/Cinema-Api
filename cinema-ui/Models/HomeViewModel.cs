using cinema_ui.Services;

namespace cinema_ui.Models;

public class HomeViewModel
{
    public List<cinema_ui.Services.Movie> Movies { get; set; } = new();
    public List<cinema_ui.Services.Screening> Screenings { get; set; } = new();
}

