using api_cinema.Data;
using api_cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace api_cinema.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class PromoCodeController : ControllerBase
{
    private readonly AppDbContext _db;

    public PromoCodeController(AppDbContext db)
    {
        _db = db;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    // GET: api/PromoCode - Get all promo codes
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var promoCodes = await _db.PromoCodes
            .Include(p => p.CreatedBy)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PromoCodeResponseDto
            {
                Id = p.Id,
                Code = p.Code,
                Description = p.Description,
                DiscountPercent = p.DiscountPercent,
                MaxDiscountAmount = p.MaxDiscountAmount,
                MaxUses = p.MaxUses,
                CurrentUses = p.CurrentUses,
                MinPurchaseAmount = p.MinPurchaseAmount,
                ValidFrom = p.ValidFrom,
                ExpiresAt = p.ExpiresAt,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy.Username
            })
            .ToListAsync();

        return Ok(promoCodes);
    }

    // GET: api/PromoCode/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var promoCode = await _db.PromoCodes
            .Include(p => p.CreatedBy)
            .Where(p => p.Id == id)
            .Select(p => new PromoCodeResponseDto
            {
                Id = p.Id,
                Code = p.Code,
                Description = p.Description,
                DiscountPercent = p.DiscountPercent,
                MaxDiscountAmount = p.MaxDiscountAmount,
                MaxUses = p.MaxUses,
                CurrentUses = p.CurrentUses,
                MinPurchaseAmount = p.MinPurchaseAmount,
                ValidFrom = p.ValidFrom,
                ExpiresAt = p.ExpiresAt,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                CreatedBy = p.CreatedBy.Username
            })
            .FirstOrDefaultAsync();

        if (promoCode == null)
            return NotFound("Promo code not found.");

        return Ok(promoCode);
    }

    // POST: api/PromoCode - Create new promo code
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PromoCodeDto dto)
    {
        // Validate code uniqueness
        var existingCode = await _db.PromoCodes.AnyAsync(p => p.Code == dto.Code.ToUpper());
        if (existingCode)
            return BadRequest("A promo code with this code already exists.");

        if (dto.DiscountPercent <= 0 || dto.DiscountPercent > 100)
            return BadRequest("Discount percent must be between 0 and 100.");

        var promoCode = new PromoCode
        {
            Code = dto.Code.ToUpper(),
            Description = dto.Description,
            DiscountPercent = dto.DiscountPercent,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            MaxUses = dto.MaxUses,
            MinPurchaseAmount = dto.MinPurchaseAmount,
            ValidFrom = dto.ValidFrom,
            ExpiresAt = dto.ExpiresAt,
            IsActive = true,
            CreatedByUserId = GetUserId()
        };

        _db.PromoCodes.Add(promoCode);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = promoCode.Id }, new { promoCode.Id, promoCode.Code });
    }

    // PUT: api/PromoCode/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PromoCodeDto dto)
    {
        var promoCode = await _db.PromoCodes.FindAsync(id);
        if (promoCode == null)
            return NotFound("Promo code not found.");

        // Check if code is being changed and if new code already exists
        if (dto.Code.ToUpper() != promoCode.Code)
        {
            var existingCode = await _db.PromoCodes.AnyAsync(p => p.Code == dto.Code.ToUpper() && p.Id != id);
            if (existingCode)
                return BadRequest("A promo code with this code already exists.");
            promoCode.Code = dto.Code.ToUpper();
        }

        if (dto.DiscountPercent <= 0 || dto.DiscountPercent > 100)
            return BadRequest("Discount percent must be between 0 and 100.");

        promoCode.Description = dto.Description;
        promoCode.DiscountPercent = dto.DiscountPercent;
        promoCode.MaxDiscountAmount = dto.MaxDiscountAmount;
        promoCode.MaxUses = dto.MaxUses;
        promoCode.MinPurchaseAmount = dto.MinPurchaseAmount;
        promoCode.ValidFrom = dto.ValidFrom;
        promoCode.ExpiresAt = dto.ExpiresAt;

        await _db.SaveChangesAsync();

        return Ok(new { Message = "Promo code updated successfully." });
    }

    // POST: api/PromoCode/{id}/toggle - Toggle active status
    [HttpPost("{id}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var promoCode = await _db.PromoCodes.FindAsync(id);
        if (promoCode == null)
            return NotFound("Promo code not found.");

        promoCode.IsActive = !promoCode.IsActive;
        await _db.SaveChangesAsync();

        return Ok(new { Message = $"Promo code {(promoCode.IsActive ? "activated" : "deactivated")}.", IsActive = promoCode.IsActive });
    }

    // DELETE: api/PromoCode/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var promoCode = await _db.PromoCodes.FindAsync(id);
        if (promoCode == null)
            return NotFound("Promo code not found.");

        // Check if promo code has been used
        if (promoCode.CurrentUses > 0)
            return BadRequest("Cannot delete a promo code that has been used. Deactivate it instead.");

        _db.PromoCodes.Remove(promoCode);
        await _db.SaveChangesAsync();

        return Ok(new { Message = "Promo code deleted successfully." });
    }

    // GET: api/PromoCode/validate/{code} - Validate a promo code (public endpoint for ticket purchase)
    [HttpGet("validate/{code}")]
    [AllowAnonymous]
    [Authorize(Policy = "Authenticated")]
    public async Task<IActionResult> Validate(string code, [FromQuery] decimal? purchaseAmount)
    {
        var promoCode = await _db.PromoCodes
            .FirstOrDefaultAsync(p => p.Code == code.ToUpper());

        if (promoCode == null)
            return Ok(new PromoCodeValidationDto { IsValid = false, ErrorMessage = "Invalid promo code." });

        if (!promoCode.IsActive)
            return Ok(new PromoCodeValidationDto { IsValid = false, ErrorMessage = "This promo code is no longer active." });

        if (promoCode.ValidFrom.HasValue && promoCode.ValidFrom > DateTime.UtcNow)
            return Ok(new PromoCodeValidationDto { IsValid = false, ErrorMessage = "This promo code is not yet valid." });

        if (promoCode.ExpiresAt.HasValue && promoCode.ExpiresAt < DateTime.UtcNow)
            return Ok(new PromoCodeValidationDto { IsValid = false, ErrorMessage = "This promo code has expired." });

        if (promoCode.MaxUses.HasValue && promoCode.CurrentUses >= promoCode.MaxUses)
            return Ok(new PromoCodeValidationDto { IsValid = false, ErrorMessage = "This promo code has reached its usage limit." });

        if (promoCode.MinPurchaseAmount.HasValue && purchaseAmount.HasValue && purchaseAmount < promoCode.MinPurchaseAmount)
            return Ok(new PromoCodeValidationDto { IsValid = false, ErrorMessage = $"Minimum purchase amount is ${promoCode.MinPurchaseAmount}." });

        return Ok(new PromoCodeValidationDto
        {
            IsValid = true,
            DiscountPercent = promoCode.DiscountPercent,
            MaxDiscountAmount = promoCode.MaxDiscountAmount
        });
    }
}
