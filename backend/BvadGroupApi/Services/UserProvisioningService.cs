using BvadGroupApi.Data;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    public interface IUserProvisioningService
    {
        Task<User> CreateUserForEmployeeAsync(Employee employee);
        string GenerateUsername(string firstName, string lastName);
    }

    /// <summary>
    /// Crée automatiquement un compte User quand on crée un Employee.
    /// </summary>
    public class UserProvisioningService : IUserProvisioningService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserProvisioningService> _logger;

        // Mot de passe temporaire par défaut
        private const string DefaultPassword = "Bvad2025!";

        public UserProvisioningService(AppDbContext context, ILogger<UserProvisioningService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> CreateUserForEmployeeAsync(Employee employee)
        {
            var username = GenerateUsername(employee.FirstName, employee.LastName);

            // S'assurer que le username est unique
            var baseUsername = username;
            int suffix = 1;
            while (await _context.Users.AnyAsync(u => u.Username == username))
            {
                username = $"{baseUsername}{suffix}";
                suffix++;
            }

            var user = new User
            {
                Username = username,
                Email = employee.Email,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                PhoneNumber = employee.PhoneNumber,
                PhotoUrl = employee.PhotoUrl,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword),
                Role = MapRole(employee.CompanyRole),
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Lier user ↔ filiale de l'employé
            _context.UserCompanies.Add(new UserCompany
            {
                UserId = user.Id,
                CompanyId = employee.CompanyId,
                CompanyRole = employee.CompanyRole,
                IsDefault = true
            });

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "✅ Compte User créé pour employé {Name} : username={Username}, password={Pwd}",
                employee.FullName, username, DefaultPassword);

            return user;
        }

        public string GenerateUsername(string firstName, string lastName)
        {
            // Ex : Paul Mbarga → p.mbarga
            var first = (firstName ?? "").Trim().ToLowerInvariant();
            var last = (lastName ?? "").Trim().ToLowerInvariant()
                .Replace(" ", "").Replace("-", "").Replace("'", "");

            if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(last))
                return $"user{Guid.NewGuid().ToString("N")[..8]}";

            return $"{first[0]}.{last}";
        }

        // Mapping simple : rôle filiale → rôle global (basique)
        private UserRole MapRole(UserRole companyRole)
        {
            return companyRole switch
            {
                UserRole.Director => UserRole.User,   // Reste User global
                UserRole.Manager => UserRole.User,
                UserRole.HR => UserRole.User,
                UserRole.Accountant => UserRole.User,
                _ => UserRole.User
            };
        }
    }
}