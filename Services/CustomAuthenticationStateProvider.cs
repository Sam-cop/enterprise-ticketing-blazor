using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EnterpriseTicketing.Data;
using EnterpriseTicketing.Models;
using Newtonsoft.Json;

namespace EnterpriseTicketing.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(
        ProtectedSessionStorage sessionStorage, 
        ApplicationDbContext context,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _sessionStorage = sessionStorage;
        _context = context;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userDataResult = await _sessionStorage.GetAsync<string>("UserSession");
            
            if (userDataResult.Success && !string.IsNullOrEmpty(userDataResult.Value))
            {
                var userData = JsonConvert.DeserializeObject<UserSessionData>(userDataResult.Value);
                if (userData != null)
                {
                    // Create claims from stored user data without database lookup
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userData.Id.ToString()),
                        new Claim(ClaimTypes.Email, userData.Email),
                        new Claim(ClaimTypes.GivenName, userData.FirstName),
                        new Claim(ClaimTypes.Surname, userData.LastName),
                        new Claim(ClaimTypes.Role, userData.Role.ToString())
                    };

                    var identity = new ClaimsIdentity(claims, "custom");
                    _currentUser = new ClaimsPrincipal(identity);
                    
                    return new AuthenticationState(_currentUser);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving authentication state");
        }

        // Return anonymous user if no valid session found
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        return new AuthenticationState(_currentUser);
    }

    public async Task MarkUserAsAuthenticatedAsync(User user)
    {
        try
        {
            var userData = new UserSessionData
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            };

            await _sessionStorage.SetAsync("UserSession", JsonConvert.SerializeObject(userData));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, "custom");
            _currentUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking user as authenticated");
        }
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        try
        {
            await _sessionStorage.DeleteAsync("UserSession");
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging out user");
        }
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            var authState = await GetAuthenticationStateAsync();
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    // Verify user still exists and is active in the database
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
                    if (user == null)
                    {
                        // User no longer exists or is inactive, clear the session
                        await MarkUserAsLoggedOutAsync();
                    }
                    return user;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
        }
        
        return null;
    }
}

public class UserSessionData
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}