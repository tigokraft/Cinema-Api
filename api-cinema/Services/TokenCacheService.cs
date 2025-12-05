using Microsoft.Extensions.Caching.Memory;

namespace api_cinema.Services;

public class TokenCacheService
{
    private readonly IMemoryCache _cache;
    private const int CacheExpirationMinutes = 1440; // 24 hours

    public TokenCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void CacheToken(string token, int userId, string username, string role)
    {
        var cacheKey = $"token_{userId}_{username}";
        var cacheEntry = new CachedTokenInfo
        {
            Token = token,
            UserId = userId,
            Username = username,
            Role = role,
            CachedAt = DateTime.UtcNow
        };

        _cache.Set(cacheKey, cacheEntry, TimeSpan.FromMinutes(CacheExpirationMinutes));
    }

    public CachedTokenInfo? GetCachedToken(int userId, string username)
    {
        var cacheKey = $"token_{userId}_{username}";
        return _cache.Get<CachedTokenInfo>(cacheKey);
    }

    public void InvalidateToken(int userId, string username)
    {
        var cacheKey = $"token_{userId}_{username}";
        _cache.Remove(cacheKey);
    }

    public void InvalidateAllUserTokens(int userId)
    {
        // Note: MemoryCache doesn't support wildcard removal directly
        // For production with many users, consider using a distributed cache (Redis)
        // or maintaining a list of cache keys per user in a separate collection
        // This method is kept for future implementation
    }
}

public class CachedTokenInfo
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CachedAt { get; set; }
}

