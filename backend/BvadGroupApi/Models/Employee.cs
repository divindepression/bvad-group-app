namespace BvadGroupApi.Models
{
    /// <summary>
    /// Représente un employé du groupe BVAD.
    /// Un employé appartient à une filiale principale mais peut collaborer avec d'autres.
    /// </summary>
    public class Employee
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // 📇 Identité
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public Gender Gender { get; set; } = Gender.Male;
        public DateTime? BirthDate { get; set; }
        public string? BirthPlace { get; set; }
        public string? Nationality { get; set; }

        // 📞 Contact
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        // 💼 Emploi
        public string Position { get; set; } = string.Empty;          // Poste (ex: "Développeur senior")
        public string? Department { get; set; }                        // Département (ex: "IT")
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public ContractType ContractType { get; set; } = ContractType.CDI;
        public decimal? Salary { get; set; }                           // Salaire brut mensuel
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        // 🏢 Filiale principale
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // 🖼 Photo
        public string? PhotoUrl { get; set; }

        // 🔗 Lien optionnel avec compte User (si l'employé peut se connecter)
        public Guid? UserId { get; set; }
        public User? User { get; set; }

        // 📝 Notes
        public string? Notes { get; set; }

        // 🕒 Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 🧮 Calculé
        public string FullName => $"{FirstName} {(MiddleName ?? "")} {LastName}".Trim().Replace("  ", " ");
        public int? Age => BirthDate.HasValue
            ? (int)((DateTime.UtcNow - BirthDate.Value).TotalDays / 365.25)
            : null;
    }
}