using System.ComponentModel.DataAnnotations;

namespace api_cinema.Models;

public class MovieDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Genre { get; set; } = string.Empty;
    
    [Range(1, 600)]
    public int DurationMinutes { get; set; }
    
    [Required]
    public DateTime ReleaseDate { get; set; }
    
    [StringLength(100)]
    public string Director { get; set; } = string.Empty;
    
    public string? PosterUrl { get; set; }
    
    public string? BackdropUrl { get; set; }
    
    public string? TrailerUrl { get; set; }
    
    [StringLength(10)]
    public string Rating { get; set; } = "NR";
    
    public bool IsFeatured { get; set; } = false;
    
    public bool ShowTrailer { get; set; } = true;
}

public class MovieResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Director { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string Rating { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public bool ShowTrailer { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasDownloadedTrailer { get; set; }
}


