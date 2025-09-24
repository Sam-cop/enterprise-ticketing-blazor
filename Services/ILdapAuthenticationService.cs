using EnterpriseTicketing.Models;

namespace EnterpriseTicketing.Services;

public interface ILdapAuthenticationService
{
    Task<User?> AuthenticateAsync(string email, string password);
    Task<User?> GetUserByEmailAsync(string email);
}