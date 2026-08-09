using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BvadGroupApi.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext context,
            IConfiguration config,
            ILogger<AuthService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            // 🔍 Chercher l'utilisateur avec ses filiales
            var user = await _context.Users
                .Include(u => u.UserCompanies)
                    .ThenInclude(uc => uc.Company)
                .FirstOrDefaultAsync(u =>
                    u.Username == request.Username || u.Email == request.Username);

            if (user == null)
            {
                _logger.LogWarning("❌ Login échoué : utilisateur '{Username}' introuvable", request.Username);
                return null;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("❌ Login échoué : utilisateur '{Username}' désactivé", request.Username);
                return null;
            }

            // 🔒 Vérifier le mot de passe
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("❌ Login échoué : mauvais mot de passe pour '{Username}'", request.Username);
                return null;
            }

            // ✅ Login réussi → mettre à jour LastLoginAt
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 🎫 Générer le token JWT
            var (token, expiresAt) = GenerateJwtToken(user);

            _logger.LogInformation("✅ Login réussi pour '{Username}'", user.Username);

            // 📦 Construire la réponse
            return new LoginResponse(
                Token: token,
                ExpiresAt: expiresAt,
                User: new UserDto(
                    user.Id,
                    user.Username,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.FullName,
                    user.Role.ToString(),
                    user.PhotoUrl
                ),
                Companies: user.UserCompanies
                    .OrderBy(uc => uc.Company.DisplayOrder)
                    .Select(uc => new CompanyAccessDto(
                        uc.Company.Id,
                        uc.Company.Code,
                        uc.Company.Name,
                        uc.Company.Color,
                        uc.Company.Logo,
                        uc.Company.IsHolding,
                        uc.IsDefault,
                        uc.CompanyRole?.ToString()
                    ))
                    .ToList()
            );
        }

        // ================================================
        // 🎫 Génération du JWT
        // ================================================
        private (string token, DateTime expiresAt) GenerateJwtToken(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(jwtSection["ExpiresInMinutes"]!));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("firstName", user.FirstName),
                new("lastName", user.LastName)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenStr, expiresAt);
        }
    }
}