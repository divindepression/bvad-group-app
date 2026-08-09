using System.Reflection;

namespace BvadGroupApi.Models
{
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
        public string Position { get; set; } = string.Empty;
        public string? Department { get; set; }
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public ContractType ContractType { get; set; } = ContractType.CDI;
        public decimal? Salary { get; set; }
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        // 🎭 Rôle dans la filiale
        public UserRole CompanyRole { get; set; } = UserRole.Employee;

        // 🏛 Comité de direction
        public bool IsCommitteeMember { get; set; } = false;
        public CommitteePosition CommitteePosition { get; set; } = CommitteePosition.None;
        public string? CommitteePositionCustom { get; set; }  // Si CommitteePosition == Custom

        // 🌳 Hiérarchie
        public Guid? ManagerId { get; set; }
        public Employee? Manager { get; set; }

        // 🏢 Filiale
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // 🖼 Photo
        public string? PhotoUrl { get; set; }

        // 🔗 Compte User associé (auto-créé)
        public Guid? UserId { get; set; }
        public User? User { get; set; }

        // 📝 Notes internes
        public string? Notes { get; set; }

        // 🕒 Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 🧮 Calculé
        public string FullName => $"{FirstName} {(MiddleName ?? "")} {LastName}"
            .Trim().Replace("  ", " ");

        public int? Age => BirthDate.HasValue
            ? (int)((DateTime.UtcNow - BirthDate.Value).TotalDays / 365.25)
            : null;
    }
}