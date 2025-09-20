using Microsoft.Extensions.Caching.Hybrid;

namespace MS.API.Mini.Data;

public class IOrgContext(IHttpContextAccessor httpContextAccessor, HybridCache cache, MonitorDBContext dbCtx)
{
    private const string UserIdClaim = "";
    
    private Guid UserId
    {
        get
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == UserIdClaim)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User not found");
            }

            return userId;
        }
    }

    public async Task<Guid> GetOrgIdAsync()
    {
        var cacheKey = $"orgId_{UserId}";
        return await cache.GetOrCreateAsync(cacheKey, async ct =>
        {
            var user = await dbCtx.Users.Where(u => u.UserId == UserId).Select(u => new { u.OrganizationId }).FirstOrDefaultAsync(ct);
            
            return user?.OrganizationId ?? throw new Exception("User not found");
        }, new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(30)
        });
    }
}