namespace api_cinema.Models;

public class PromoCode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DiscountPercent { get; set; } // 0-100
    public decimal? MaxDiscountAmount { get; set; } // Cap on discount
    public int? MaxUses { get; set; } // null = unlimited
    public int CurrentUses { get; set; } = 0;
    public decimal? MinPurchaseAmount { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }
    
    // Navigation properties
    public User CreatedBy { get; set; } = null!;
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
