using BvadGroupApi.Data;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    /// <summary>
    /// Initialise la base avec les données par défaut au premier démarrage.
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(AppDbContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            // ✅ Appliquer les migrations si besoin
            await _context.Database.MigrateAsync();

            await SeedCompaniesAsync();
            await SeedSuperAdminAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Seed terminé");
        }

        // ================================================
        // 🏢 SEED DES FILIALES
        // ================================================
        private async Task SeedCompaniesAsync()
        {
            if (await _context.Companies.AnyAsync())
            {
                _logger.LogInformation("🏢 Filiales déjà présentes, seed ignoré");
                return;
            }

            _logger.LogInformation("🏢 Création des filiales BVAD GROUP...");

            var companies = new List<Company>
            {
                new()
                {
                    Code = "BVAD_GROUP",
                    Name = "BVAD GROUP",
                    Description = "Bâtir. Valoriser. Accompagner. Développer.",
                    Color = "#1e3a8a",
                    Logo = "🏢",
                    IsHolding = true,
                    DisplayOrder = 0
                },
                new()
                {
                    Code = "BVAD_AGRO",
                    Name = "BVAD Agro",
                    Description = "Agriculture, élevage et production agroalimentaire",
                    Color = "#16a34a",
                    Logo = "🌾",
                    DisplayOrder = 1
                },
                new()
                {
                    Code = "BVAD_TECH",
                    Name = "BVAD Tech",
                    Description = "Solutions technologiques et développement logiciel",
                    Color = "#0891b2",
                    Logo = "💻",
                    DisplayOrder = 2
                },
                new()
                {
                    Code = "BVAD_SCHOOL",
                    Name = "BVAD School",
                    Description = "Formation, éducation et accompagnement pédagogique",
                    Color = "#ea580c",
                    Logo = "🎓",
                    DisplayOrder = 3
                },
                new()
                {
                    Code = "BVAD_CONSEIL",
                    Name = "BVAD Conseil",
                    Description = "Conseil stratégique et accompagnement des entreprises",
                    Color = "#7c3aed",
                    Logo = "💼",
                    DisplayOrder = 4
                }
            };

            await _context.Companies.AddRangeAsync(companies);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ {Count} filiales créées", companies.Count);
        }

        // ================================================
        // 👤 SEED DU SUPER ADMIN
        // ================================================
        private async Task SeedSuperAdminAsync()
        {
            if (await _context.Users.AnyAsync())
            {
                _logger.LogInformation("👤 Utilisateurs déjà présents, seed ignoré");
                return;
            }

            _logger.LogInformation("👤 Création du SuperAdmin...");

            var superAdmin = new User
            {
                Username = "divin",
                Email = "divin@bvad-group.com",
                FirstName = "Divin",
                LastName = "BVAD",
                PhoneNumber = "+237600000000",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Bvad2025!"),
                Role = UserRole.SuperAdmin,
                IsActive = true
            };

            await _context.Users.AddAsync(superAdmin);
            await _context.SaveChangesAsync();

            // 🔗 Donner accès à TOUTES les filiales
            var companies = await _context.Companies.ToListAsync();
            foreach (var company in companies)
            {
                _context.UserCompanies.Add(new UserCompany
                {
                    UserId = superAdmin.Id,
                    CompanyId = company.Id,
                    CompanyRole = UserRole.SuperAdmin,
                    IsDefault = company.IsHolding  // Filiale par défaut = holding
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ SuperAdmin 'divin' créé avec accès à toutes les filiales");
            _logger.LogInformation("🔑 Mot de passe par défaut : Bvad2025!");
        }
    }
}