using System.DirectoryServices.Protocols;
using System.Net;
using Microsoft.EntityFrameworkCore;
using EnterpriseTicketing.Data;
using EnterpriseTicketing.Models;

namespace EnterpriseTicketing.Services;

public class LdapAuthenticationService : ILdapAuthenticationService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LdapAuthenticationService> _logger;

    public LdapAuthenticationService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<LdapAuthenticationService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        try
        {
            // Check for admin user first (hardcoded for demo)
            if (email == "admin@mail.com" && password == "!123!QWEqwe")
            {
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (adminUser != null)
                {
                    return adminUser;
                }
            }

            // For demo purposes, we'll simulate LDAP authentication
            // In a real scenario, you would connect to an actual LDAP server
            var ldapServer = _configuration["LDAP:Server"] ?? "localhost";
            var ldapPort = int.Parse(_configuration["LDAP:Port"] ?? "389");
            var ldapBaseDn = _configuration["LDAP:BaseDN"] ?? "dc=example,dc=com";

            if (await SimulateLdapAuthenticationAsync(email, password, ldapServer, ldapPort, ldapBaseDn))
            {
                // Check if user exists in database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                
                if (user == null)
                {
                    // Create new user from LDAP info
                    user = await CreateUserFromLdapAsync(email);
                    if (user != null)
                    {
                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();
                    }
                }

                return user;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LDAP authentication for user {Email}", email);
            return null;
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    private async Task<bool> SimulateLdapAuthenticationAsync(string email, string password, string server, int port, string baseDn)
    {
        // This is a simulation for demo purposes
        // In a real implementation, you would use LdapConnection to authenticate against an actual LDAP server
        
        try
        {
            // For demo purposes, accept any user with a valid email format and non-empty password
            // except for the admin user which has specific credentials
            if (email == "admin@mail.com")
            {
                return password == "!123!QWEqwe";
            }

            // Simulate successful authentication for valid email formats
            if (IsValidEmail(email) && !string.IsNullOrWhiteSpace(password) && password.Length >= 6)
            {
                // Simulate network delay
                await Task.Delay(100);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LDAP authentication simulation failed");
            return false;
        }
    }

    private async Task<User?> CreateUserFromLdapAsync(string email)
    {
        try
        {
            // In a real LDAP implementation, you would query the LDAP server for user details
            // For demo purposes, we'll create a basic user
            var parts = email.Split('@');
            var firstName = parts[0].Split('.').FirstOrDefault() ?? "User";
            var lastName = parts[0].Split('.').Skip(1).FirstOrDefault() ?? "Unknown";

            return new User
            {
                Email = email,
                FirstName = char.ToUpper(firstName[0]) + firstName.Substring(1).ToLower(),
                LastName = char.ToUpper(lastName[0]) + lastName.Substring(1).ToLower(),
                Department = "General",
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user from LDAP for {Email}", email);
            return null;
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}