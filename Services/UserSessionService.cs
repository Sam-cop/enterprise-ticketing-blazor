using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using EnterpriseTicketing.Data;
using EnterpriseTicketing.Models;

namespace EnterpriseTicketing.Services;

public class UserSessionService : IUserSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserSessionService> _logger;

    public UserSessionService(
        IHttpContextAccessor httpContextAccessor,
        ApplicationDbContext context,
        ILogger<UserSessionService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            var userJson = session.GetString("CurrentUser");
            if (string.IsNullOrEmpty(userJson)) return null;

            var userData = JsonConvert.DeserializeObject<dynamic>(userJson);
            if (userData == null) return null;

            int userId = userData.Id;
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user from session");
            return null;
        }
    }

    public async Task SetCurrentUserAsync(User user)
    {
        try
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            var userData = new
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            };

            session.SetString("CurrentUser", JsonConvert.SerializeObject(userData));
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting current user in session");
        }
    }

    public async Task ClearCurrentUserAsync()
    {
        try
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove("CurrentUser");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing current user from session");
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var user = await GetCurrentUserAsync();
        return user != null;
    }
}