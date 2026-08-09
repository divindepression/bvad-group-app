namespace BvadGroupApi.Models
{
    public class Employee
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // ═══════════════════════════════════════
        // 🆔 IDENTIFIANT INTERNE
        // ═══════════════════════════════════════
        public string? EmployeeNumber { get; set; }  // Matricule auto (ex: BVAD-TECH-2025-003)

        // ═══════════════════════════════════════
        // 📇 IDENTITÉ
        // ═══════════════════════════════════════
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public Gender Gender { get; set; } = Gender.Male;
        public DateTime? BirthDate { get; set; }
        public string? BirthPlace { get; set; }
        public string? Nationality { get; set; }
        public string? MaritalStatus { get; set; }         // Célibataire, Marié(e), Divorcé(e)...
        public int? NumberOfChildren { get; set; }

        // 🆔 Pièces d'identité
        public string? NationalIdNumber { get; set; }      // N° CNI
        public DateTime? NationalIdExpiry { get; set; }
        public string? PassportNumber { get; set; }
        public string? SocialSecurityNumber { get; set; }   // CNPS pour Cameroun

        // ═══════════════════════════════════════
        // 📞 CONTACT
        // ═══════════════════════════════════════
        public string Email { get; set; } = string.Empty;
        public string? PersonalEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? SecondaryPhone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        // ═══════════════════════════════════════
        // 🚨 CONTACT D'URGENCE
        // ═══════════════════════════════════════
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelation { get; set; }  // Père, mère, conjoint...

        // ═══════════════════════════════════════
        // 🏦 COORDONNÉES BANCAIRES
        // ═══════════════════════════════════════
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankIban { get; set; }
        public string? BankSwift { get; set; }
        public string? PaymentMethod { get; set; }  // Virement, Mobile Money, Espèces

        // 📱 Mobile Money (Cameroun)
        public string? MobileMoneyOperator { get; set; }  // MTN, Orange
        public string? MobileMoneyNumber { get; set; }

        // ═══════════════════════════════════════
        // 💼 EMPLOI
        // ═══════════════════════════════════════
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
        public string? CommitteePositionCustom { get; set; }

        // 🌳 Hiérarchie
        public Guid? ManagerId { get; set; }
        public Employee? Manager { get; set; }

        // 🏢 Filiale
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // ═══════════════════════════════════════
        // 🖼 PHOTOS ET SIGNATURE
        // ═══════════════════════════════════════
        public string? PhotoUrl { get; set; }                // Photo profil
        public string? IdentityPhotoUrl { get; set; }        // Photo identité officielle (pour badge)
        public string? SignatureUrl { get; set; }            // Signature scannée

        // 🎫 Badge
        public DateTime? BadgeValidUntil { get; set; }       // Date de validité badge

        // ═══════════════════════════════════════
        // 🔗 COMPTE USER
        // ═══════════════════════════════════════
        public Guid? UserId { get; set; }
        public User? User { get; set; }

        // ═══════════════════════════════════════
        // 📝 NOTES
        // ═══════════════════════════════════════
        public string? Notes { get; set; }

        // ═══════════════════════════════════════
        // 🕒 AUDIT
        // ═══════════════════════════════════════
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ═══════════════════════════════════════
        // 🧮 CALCULÉ
        // ═══════════════════════════════════════
        public string FullName => $"{FirstName} {(MiddleName ?? "")} {LastName}"
            .Trim().Replace("  ", " ");

        public int? Age => BirthDate.HasValue
            ? (int)((DateTime.UtcNow - BirthDate.Value).TotalDays / 365.25)
            : null;

        public int YearsInCompany => (int)((DateTime.UtcNow - HireDate).TotalDays / 365.25);
    }
}