using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using EnterpriseTicketing.Data;
using EnterpriseTicketing.Models;

namespace EnterpriseTicketing.Services;

public class UserSessionService : IUserSessionService
{
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    private readonly ILogger<UserSessionService> _logger;

    public UserSessionService(
        AuthenticationStateProvider authStateProvider,
        ILogger<UserSessionService> logger)
    {
        _authStateProvider = (CustomAuthenticationStateProvider)authStateProvider;
        _logger = logger;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            return await _authStateProvider.GetCurrentUserAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return null;
        }
    }

    public async Task SetCurrentUserAsync(User user)
    {
        try
        {
            await _authStateProvider.MarkUserAsAuthenticatedAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting current user");
        }
    }

    public async Task ClearCurrentUserAsync()
    {
        try
        {
            await _authStateProvider.MarkUserAsLoggedOutAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing current user");
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var user = await GetCurrentUserAsync();
        return user != null;
    }
}