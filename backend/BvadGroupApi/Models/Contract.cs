namespace BvadGroupApi.Models
{
    public enum ContractStatus
    {
        Draft = 0,          // Brouillon
        Active = 1,         // En cours
        Suspended = 2,      // Suspendu
        Terminated = 3,     // Rompu
        Expired = 4,        // Arrivé à échéance
        Renewed = 5         // Renouvelé
    }

    /// <summary>
    /// Contrat de travail d'un employé.
    /// Un employé peut avoir plusieurs contrats (historique).
    /// </summary>
    public class Contract
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // 🔗 Employé concerné
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // 🏢 Filiale (dénormalisé pour requêtes rapides)
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // 📄 Info contrat
        public string ContractNumber { get; set; } = string.Empty;  // ex: BVAD-TECH-2025-001
        public ContractType ContractType { get; set; } = ContractType.CDI;
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        public string Position { get; set; } = string.Empty;       // Poste au contrat
        public string? Department { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public DateTime? SignedDate { get; set; }                   // Date de signature

        public decimal Salary { get; set; }                          // Salaire brut mensuel
        public string Currency { get; set; } = "FCFA";
        public int? TrialPeriodMonths { get; set; }                 // Période d'essai en mois
        public int? WeeklyHours { get; set; } = 40;

        // 📎 Fichier PDF signé
        public string? DocumentUrl { get; set; }                    // Chemin relatif
        public string? DocumentFileName { get; set; }
        public long? DocumentSize { get; set; }

        // 📝 Notes / clauses spéciales
        public string? SpecialClauses { get; set; }
        public string? Notes { get; set; }

        // 🕒 Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedById { get; set; }
        public User? CreatedBy { get; set; }

        // 🧮 Calculé
        public int? RemainingDays => EndDate.HasValue
            ? (int)(EndDate.Value - DateTime.UtcNow).TotalDays
            : null;

        public bool IsExpiringSoon => RemainingDays.HasValue && RemainingDays.Value >= 0 && RemainingDays.Value <= 30;
        public bool IsExpired => RemainingDays.HasValue && RemainingDays.Value < 0;
    }
}