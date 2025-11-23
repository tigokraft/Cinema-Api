using System.ComponentModel.DataAnnotations;

namespace api_cinema.Models;

public class TheaterDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Range(1, 1000)]
    public int Capacity { get; set; }
    
    [Range(1, 50)]
    public int Rows { get; set; }
    
    [Range(1, 50)]
    public int SeatsPerRow { get; set; }
}

public class TheaterResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
    public bool IsActive { get; set; }
}


