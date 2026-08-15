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
            await SeedLeaveTypesAsync();
            await SeedLeaveBalancesAsync();

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
        LegalName = "BVAD GROUP SARL",
        Description = "Bâtir. Valoriser. Accompagner. Développer.",
        Slogan = "Bâtir · Valoriser · Accompagner · Développer",
        Color = "#1e3a8a",
        Logo = "🏢",
        IsHolding = true,
        DisplayOrder = 0,
        Address = "BP 12345, Douala",
        City = "Douala",
        Country = "Cameroun",
        Phone = "+237 6 00 00 00 00",
        Email = "contact@bvad-group.com",
        Website = "www.bvad-group.com",
        DirectorName = "Divin BVAD",
        DirectorTitle = "Président du Groupe",
        RegistrationNumber = "RC/DLA/2025/A/0001",
        TaxNumber = "M012500000000A"
    },
    new()
    {
        Code = "BVAD_AGRO",
        Name = "BVAD Agro",
        LegalName = "BVAD AGRO SARL",
        Description = "Agriculture, élevage et production agroalimentaire",
        Slogan = "Nourrir · Cultiver · Prospérer",
        Color = "#16a34a",
        Logo = "🌾",
        DisplayOrder = 1,
        City = "Douala",
        Country = "Cameroun",
        Phone = "+237 6 00 00 00 01",
        Email = "contact@bvad-agro.com",
        DirectorName = "Jean Kamga",
        DirectorTitle = "Directeur Général"
    },
    new()
    {
        Code = "BVAD_TECH",
        Name = "BVAD Tech",
        LegalName = "BVAD TECHNOLOGIES SARL",
        Description = "Solutions technologiques et développement logiciel",
        Slogan = "Innover · Coder · Transformer",
        Color = "#0891b2",
        Logo = "💻",
        DisplayOrder = 2,
        City = "Yaoundé",
        Country = "Cameroun",
        Phone = "+237 6 00 00 00 02",
        Email = "contact@bvad-tech.com",
        DirectorName = "Paul Mbarga",
        DirectorTitle = "Directeur Technique"
    },
    new()
    {
        Code = "BVAD_SCHOOL",
        Name = "BVAD School",
        LegalName = "BVAD SCHOOL SARL",
        Description = "Formation, éducation et accompagnement pédagogique",
        Slogan = "Former · Enseigner · Élever",
        Color = "#ea580c",
        Logo = "🎓",
        DisplayOrder = 3,
        City = "Douala",
        Country = "Cameroun",
        Phone = "+237 6 00 00 00 03",
        Email = "contact@bvad-school.com",
        DirectorName = "Christine Bella",
        DirectorTitle = "Directrice Pédagogique"
    },
    new()
    {
        Code = "BVAD_CONSEIL",
        Name = "BVAD Conseil",
        LegalName = "BVAD CONSEIL SARL",
        Description = "Conseil stratégique et accompagnement des entreprises",
        Slogan = "Conseiller · Accompagner · Réussir",
        Color = "#7c3aed",
        Logo = "💼",
        DisplayOrder = 4,
        City = "Douala",
        Country = "Cameroun",
        Phone = "+237 6 00 00 00 04",
        Email = "contact@bvad-conseil.com",
        DirectorName = "Aline Tchouameni",
        DirectorTitle = "Directrice Générale"
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

            // 🆔 GÉNÉRER LES MATRICULES
            _logger.LogInformation("🆔 Génération des matricules...");

            var year = DateTime.UtcNow.Year;
            int techCount = 0, agroCount = 0, schoolCount = 0, conseilCount = 0;

            foreach (var emp in employees.OrderBy(e => e.HireDate))
            {
                var company = await _context.Companies.FindAsync(emp.CompanyId);
                if (company == null) continue;

                int number = company.Code switch
                {
                    "BVAD_TECH" => ++techCount,
                    "BVAD_AGRO" => ++agroCount,
                    "BVAD_SCHOOL" => ++schoolCount,
                    "BVAD_CONSEIL" => ++conseilCount,
                    _ => 1
                };

                emp.EmployeeNumber = $"{company.Code}-{year}-{number:D3}";
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Matricules générés (ex: BVAD_TECH-2025-001)");

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

        // ================================================
        // 🏖 SEED DES TYPES DE CONGÉS (norme Congo)
        // ================================================
        private async Task SeedLeaveTypesAsync()
        {
            if (await _context.LeaveTypes.AnyAsync())
            {
                _logger.LogInformation("🏖 Types de congés déjà présents, seed ignoré");
                return;
            }

            _logger.LogInformation("🏖 Création des types de congés (norme Congo)...");

            var types = new List<LeaveType>
    {
        new()
        {
            Code = "CP",
            Name = "Congés payés annuels",
            Description = "Congés payés selon Code du travail congolais (2 jours ouvrables/mois)",
            Icon = "🏖",
            Color = "#3b82f6",
            DefaultDaysPerYear = 26,
            DaysAccruedPerMonth = 2.0m,
            IsPaid = true,
            RequiresProof = false,
            DecrementsBalance = true,
            DisplayOrder = 1
        },
        new()
        {
            Code = "MAL",
            Name = "Congé maladie",
            Description = "Arrêt maladie sur présentation de certificat médical",
            Icon = "🤒",
            Color = "#ef4444",
            DefaultDaysPerYear = 0, // Pas de quota fixe
            DaysAccruedPerMonth = 0,
            IsPaid = true,
            RequiresProof = true,
            DecrementsBalance = false, // Pas décompté du solde
            DisplayOrder = 2
        },
        new()
        {
            Code = "MAT",
            Name = "Congé maternité",
            Description = "15 semaines (6 avant + 9 après accouchement)",
            Icon = "🤱",
            Color = "#ec4899",
            DefaultDaysPerYear = 105,
            DaysAccruedPerMonth = 0,
            IsPaid = true,
            RequiresProof = true,
            DecrementsBalance = false,
            DisplayOrder = 3
        },
        new()
        {
            Code = "PAT",
            Name = "Congé paternité",
            Description = "2 jours ouvrables à la naissance",
            Icon = "👨‍👧",
            Color = "#8b5cf6",
            DefaultDaysPerYear = 2,
            DaysAccruedPerMonth = 0,
            IsPaid = true,
            RequiresProof = true,
            DecrementsBalance = false,
            DisplayOrder = 4
        },
        new()
        {
            Code = "MAR",
            Name = "Congé mariage",
            Description = "4 jours ouvrables pour mariage de l'employé",
            Icon = "💒",
            Color = "#f59e0b",
            DefaultDaysPerYear = 4,
            DaysAccruedPerMonth = 0,
            IsPaid = true,
            RequiresProof = false,
            DecrementsBalance = false,
            DisplayOrder = 5
        },
        new()
        {
            Code = "DEC",
            Name = "Congé décès",
            Description = "3 jours pour décès conjoint/enfant/parent, 2 jours pour frère/sœur/beaux-parents",
            Icon = "🕊",
            Color = "#64748b",
            DefaultDaysPerYear = 3,
            DaysAccruedPerMonth = 0,
            IsPaid = true,
            RequiresProof = false,
            DecrementsBalance = false,
            DisplayOrder = 6
        },
        new()
        {
            Code = "FOR",
            Name = "Congé formation",
            Description = "Congé pour formation professionnelle",
            Icon = "🎓",
            Color = "#10b981",
            DefaultDaysPerYear = 0,
            DaysAccruedPerMonth = 0,
            IsPaid = true,
            RequiresProof = true,
            DecrementsBalance = false,
            DisplayOrder = 7
        },
        new()
        {
            Code = "SS",
            Name = "Congé sans solde",
            Description = "Congé non rémunéré sur accord employeur",
            Icon = "🕐",
            Color = "#94a3b8",
            DefaultDaysPerYear = 0,
            DaysAccruedPerMonth = 0,
            IsPaid = false,
            RequiresProof = false,
            DecrementsBalance = false,
            DisplayOrder = 8
        }
    };

            await _context.LeaveTypes.AddRangeAsync(types);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ {Count} types de congés créés", types.Count);
        }

        // ================================================
        // 📊 SEED DES SOLDES INITIAUX
        // ================================================
        private async Task SeedLeaveBalancesAsync()
        {
            if (await _context.LeaveBalances.AnyAsync())
            {
                _logger.LogInformation("📊 Soldes de congés déjà présents, seed ignoré");
                return;
            }

            _logger.LogInformation("📊 Attribution des soldes de congés initiaux...");

            var employees = await _context.Employees.ToListAsync();
            var cpType = await _context.LeaveTypes.FirstAsync(t => t.Code == "CP");
            var year = DateTime.UtcNow.Year;

            var balances = new List<LeaveBalance>();

            foreach (var emp in employees)
            {
                // Calculer les jours acquis selon l'ancienneté cette année
                var monthsWorked = CalculateMonthsWorkedThisYear(emp.HireDate, year);
                var allocatedDays = Math.Min(26, monthsWorked * cpType.DaysAccruedPerMonth);

                balances.Add(new LeaveBalance
                {
                    EmployeeId = emp.Id,
                    LeaveTypeId = cpType.Id,
                    Year = year,
                    AllocatedDays = allocatedDays,
                    UsedDays = 0,
                    CarriedOverDays = 0,
                    Adjustment = 0
                });
            }

            await _context.LeaveBalances.AddRangeAsync(balances);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ {Count} soldes CP créés", balances.Count);
        }

        private int CalculateMonthsWorkedThisYear(DateTime hireDate, int year)
        {
            var startOfYear = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now = DateTime.UtcNow;

            var start = hireDate > startOfYear ? hireDate : startOfYear;
            var end = now.Year == year ? now : new DateTime(year, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            if (end < start) return 0;

            var months = (end.Year - start.Year) * 12 + (end.Month - start.Month);
            return Math.Max(0, months + 1); // +1 pour compter le mois en cours
        }

    }
}