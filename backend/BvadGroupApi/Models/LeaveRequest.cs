namespace BvadGroupApi.Models
{
    /// <summary>
    /// Demande de congé d'un employé.
    /// </summary>
    public class LeaveRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // 👤 Employé demandeur
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        // 🏢 Filiale (dénormalisé pour requêtes rapides)
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // 🏖 Type de congé
        public Guid LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = null!;

        // 📅 Dates
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>Nombre de jours ouvrés (calculé auto)</summary>
        public decimal DaysCount { get; set; }

        /// <summary>Congé de demi-journée (matin/après-midi) ?</summary>
        public bool IsHalfDay { get; set; }

        // 📝 Motif
        public string? Reason { get; set; }

        // 📎 Justificatif
        public string? ProofDocumentUrl { get; set; }
        public string? ProofDocumentName { get; set; }

        // 🔄 Workflow
        public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;
        public Guid? ApprovedByUserId { get; set; }
        public User? ApprovedByUser { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovalComment { get; set; }

        // 🕒 Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 🧮 Helpers calculés
        public bool IsPast => EndDate < DateTime.UtcNow;
        public bool IsCurrent => StartDate <= DateTime.UtcNow && EndDate >= DateTime.UtcNow;
        public bool IsFuture => StartDate > DateTime.UtcNow;
    }
}