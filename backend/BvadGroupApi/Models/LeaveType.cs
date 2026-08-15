namespace BvadGroupApi.Models
{
    /// <summary>
    /// Type de congé (Congés payés, Maladie, Maternité, etc.)
    /// Configurable par filiale ou globalement.
    /// </summary>
    public class LeaveType
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Code { get; set; } = string.Empty;        // CP, MAL, MAT, PAT, etc.
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Icône (emoji)</summary>
        public string Icon { get; set; } = "🏖";

        /// <summary>Couleur d'identité (hex)</summary>
        public string Color { get; set; } = "#3b82f6";

        /// <summary>Jours par défaut alloués par an (0 = pas de quota fixe)</summary>
        public int DefaultDaysPerYear { get; set; }

        /// <summary>Jours acquis par mois travaillé (ex: 2 pour CP au Congo)</summary>
        public decimal DaysAccruedPerMonth { get; set; }

        /// <summary>Le congé est-il payé ?</summary>
        public bool IsPaid { get; set; } = true;

        /// <summary>Justificatif requis ?</summary>
        public bool RequiresProof { get; set; }

        /// <summary>Décompté du solde ? (sinon illimité comme maladie)</summary>
        public bool DecrementsBalance { get; set; } = true;

        /// <summary>Ordre d'affichage</summary>
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>Filiale spécifique (null = global à tout BVAD GROUP)</summary>
        public Guid? CompanyId { get; set; }
        public Company? Company { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}