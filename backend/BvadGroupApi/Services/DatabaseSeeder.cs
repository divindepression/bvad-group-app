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
        private readonly IUserProvisioningService _userProvisioning;

        public DatabaseSeeder(AppDbContext context, ILogger<DatabaseSeeder> logger, IUserProvisioningService userProvisioning)
        {
            _context = context;
            _logger = logger;
            _userProvisioning = userProvisioning;
        }

        public async Task SeedAsync()
        {
            // ✅ Appliquer les migrations si besoin
            await _context.Database.MigrateAsync();

            await SeedCompaniesAsync();
            await SeedSuperAdminAsync();
            await SeedEmployeesAsync();

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

        // ================================================
        // 👨‍💼 SEED DES EMPLOYÉS DE TEST
        // ================================================
        private async Task SeedEmployeesAsync()
        {
            if (await _context.Employees.AnyAsync())
            {
                _logger.LogInformation("👨‍💼 Employés déjà présents, seed ignoré");
                return;
            }

            _logger.LogInformation("👨‍💼 Création des employés de test...");

            var agro = await _context.Companies.FirstAsync(c => c.Code == "BVAD_AGRO");
            var tech = await _context.Companies.FirstAsync(c => c.Code == "BVAD_TECH");
            var school = await _context.Companies.FirstAsync(c => c.Code == "BVAD_SCHOOL");
            var conseil = await _context.Companies.FirstAsync(c => c.Code == "BVAD_CONSEIL");

            var employees = new List<Employee>
{
    // 🌾 BVAD Agro
    new()
    {
        FirstName = "Jean", LastName = "Kamga",
        Email = "j.kamga@bvad-agro.com",
        PhoneNumber = "+237677000001",
        Position = "Directeur Général",
        Department = "Direction",
        CompanyId = agro.Id,
        BirthDate = new DateTime(1985, 5, 12, 0, 0, 0, DateTimeKind.Utc),
        HireDate = new DateTime(2023, 3, 1, 0, 0, 0, DateTimeKind.Utc),
        ContractType = ContractType.CDI,
        Salary = 800000,
        Status = EmployeeStatus.Active,
        City = "Douala", Country = "Cameroun",
        Gender = Gender.Male,
        CompanyRole = UserRole.Director,
        IsCommitteeMember = true,
        CommitteePosition = CommitteePosition.CEO
    },
    new()
    {
        FirstName = "Marie", LastName = "Nkomo",
        Email = "m.nkomo@bvad-agro.com",
        PhoneNumber = "+237677000002",
        Position = "Directrice RH",
        Department = "RH",
        CompanyId = agro.Id,
        BirthDate = new DateTime(1990, 8, 25, 0, 0, 0, DateTimeKind.Utc),
        HireDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
        ContractType = ContractType.CDI,
        Salary = 500000,
        Status = EmployeeStatus.Active,
        City = "Yaoundé", Country = "Cameroun",
        Gender = Gender.Female,
        CompanyRole = UserRole.HR,
        IsCommitteeMember = true,
        CommitteePosition = CommitteePosition.CHRO
    },

    // 💻 BVAD Tech
    new()
    {
        FirstName = "Paul", LastName = "Mbarga",
        Email = "p.mbarga@bvad-tech.com",
        PhoneNumber = "+237677000003",
        Position = "Directeur Technique",
        Department = "Ingénierie",
        CompanyId = tech.Id,
        BirthDate = new DateTime(1992, 2, 18, 0, 0, 0, DateTimeKind.Utc),
        HireDate = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc),
        ContractType = ContractType.CDI,
        Salary = 900000,
        Status = EmployeeStatus.Active,
        City = "Yaoundé", Country = "Cameroun",
        Gender = Gender.Male,
        CompanyRole = UserRole.Director,
        IsCommitteeMember = true,
        CommitteePosition = CommitteePosition.CEO
    },
    new()
    {
        FirstName = "Sarah", LastName = "Fokam",
        Email = "s.fokam@bvad-tech.com",
        PhoneNumber = "+237677000004",
        Position = "UI/UX Designer",
        Department = "Design",
        CompanyId = tech.Id,
        BirthDate = new DateTime(1995, 11, 3, 0, 0, 0, DateTimeKind.Utc),
        HireDate = new DateTime(2024, 4, 10, 0, 0, 0, DateTimeKind.Utc),
        ContractType = ContractType.CDI,
        Salary = 400000,
        Status = EmployeeStatus.Active,
        City = "Douala", Country = "Cameroun",
        Gender = Gender.Female,
        CompanyRole = UserRole.Employee
    },
    new()
    {
        FirstName = "Éric", LastName = "Ngoumou",
        Email = "e.ngoumou@bvad-tech.com",
        PhoneNumber = "+237677000005",
        Position = "Stagiaire développeur",
        Department = "Ingénierie",
        CompanyId = tech.Id,
        BirthDate = new DateTime(2001, 7, 20, 0, 0, 0, DateTimeKind.Utc),
        HireDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
        EndDate = new DateTime(2025, 7, 15, 0, 0, 0, DateTimeKind.Utc),
        ContractType = ContractType.Internship,
        Salary = 100000,
        Status = EmployeeStatus.Probation,
        City = "Yaoundé", Country = "Cameroun",
        Gender = Gender.Male,
        CompanyRole = UserRole.Employee
    },

    // 🎓 BVAD School
    new()
    {
        FirstName = "Christine", LastName = "Bella",
        Email = "c.bella@bvad-school.com",
        PhoneNumber = "+237677000006",
        Position = "Directrice pédagogique",
        Department = "Direction",
        CompanyId = school.Id,
        BirthDate = new DateTime(1980, 4, 8, 0, 0, 0, DateTimeKind.Utc),
        HireDate = new DateTime(2023, 9, 1, 0, 0, 0, DateTimeKind.Utc),
        ContractType = ContractType.CDI,
        Salary = 750000,
        Status = EmployeeStatus.Active,
        City = "Douala", Country = "Cameroun",
        Gender = Gender.Female,
        CompanyRole = UserRole.Director,
        IsCommitteeMember = true,
        CommitteePosition = CommitteePosition.CEO
    },
    new()
    {
        FirstName = "François", LastName = "Ateba",
        Email = "f.ateba@bvad-school.com",
        PhoneNumber = "+237677000007",
        Position = "Formateur senior",
        Department = "Formation",
        CompanyId = school.Id,
        BirthDate = new DateTime(1988, 12, 15, 0, 0, 0, DateTimeKind.Utc),
        HireDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
        ContractType = ContractType.CDD,
        Salary = 300000,
        Status = EmployeeStatus.OnLeave,
        City = "Yaoundé", Country = "Cameroun",
        Gender = Gender.Male,
        CompanyRole = UserRole.Employee
    },

    // 💼 BVAD Conseil
    new()
    {
        FirstName = "Aline", LastName = "Tchouameni",
        Email = "a.tchouameni@bvad-conseil.com",
        PhoneNumber = "+237677000008",
        Position = "Directrice Générale",
        Department = "Direction",
        CompanyId = conseil.Id,
        BirthDate = new DateTime(1987, 6, 22, 0, 0, 0, DateTimeKind.Utc),
        HireDate = new DateTime(2023, 11, 1, 0, 0, 0, DateTimeKind.Utc),
        ContractType = ContractType.CDI,
        Salary = 850000,
        Status = EmployeeStatus.Active,
        City = "Douala", Country = "Cameroun",
        Gender = Gender.Female,
        CompanyRole = UserRole.Director,
        IsCommitteeMember = true,
        CommitteePosition = CommitteePosition.CEO
    }
};

            await _context.Employees.AddRangeAsync(employees);
            await _context.SaveChangesAsync();

            // 🌳 LIER LES MANAGERS
            _logger.LogInformation("🌳 Création des liens hiérarchiques...");

            // Récupérer les employés par leur email (unique)
            var jean = employees.First(e => e.Email == "j.kamga@bvad-agro.com");
            var marie = employees.First(e => e.Email == "m.nkomo@bvad-agro.com");
            var paul = employees.First(e => e.Email == "p.mbarga@bvad-tech.com");
            var sarah = employees.First(e => e.Email == "s.fokam@bvad-tech.com");
            var eric = employees.First(e => e.Email == "e.ngoumou@bvad-tech.com");
            var christine = employees.First(e => e.Email == "c.bella@bvad-school.com");
            var francois = employees.First(e => e.Email == "f.ateba@bvad-school.com");

            // BVAD Agro : Jean → Marie
            marie.ManagerId = jean.Id;

            // BVAD Tech : Paul → Sarah, Paul → Éric
            sarah.ManagerId = paul.Id;
            eric.ManagerId = paul.Id;

            // BVAD School : Christine → François
            francois.ManagerId = christine.Id;

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Liens hiérarchiques créés");

            // 🎯 Auto-création des comptes User pour chaque employé
            _logger.LogInformation("👤 Auto-création des comptes User...");
            foreach (var emp in employees)
            {
                try
                {
                    var user = await _userProvisioning.CreateUserForEmployeeAsync(emp);
                    emp.UserId = user.Id;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Échec création User pour {Name}", emp.FullName);
                }
            }
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ {Count} employés créés", employees.Count);
        }

    }
}