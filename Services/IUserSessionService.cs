using EnterpriseTicketing.Models;

namespace EnterpriseTicketing.Services;

public interface IUserSessionService
{
    Task<User?> GetCurrentUserAsync();
    Task SetCurrentUserAsync(User user);
    Task ClearCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
}