using cinema_ui.Services;

namespace cinema_ui.Models;

public class HomeViewModel
{
    public List<Movie> Movies { get; set; } = new();
    public List<Screening> Screenings { get; set; } = new();
    public MovieResponseDto? FeaturedMovie { get; set; }
}
