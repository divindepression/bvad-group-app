namespace BvadGroupApi.Models
{
    /// <summary>
    /// Représente une entité du groupe BVAD (Holding ou Filiale)
    /// </summary>
    public class Company
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Identité
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Slogan { get; set; }              // 🆕 Ex: "Bâtir · Valoriser"

        // Design
        public string Color { get; set; } = "#1e3a8a";
        public string? Logo { get; set; }                // Emoji (fallback)
        public string? LogoUrl { get; set; }             // 🆕 URL vers image logo officiel
        public string? StampUrl { get; set; }            // 🆕 Cachet officiel (PNG transparent)
        public string? DirectorSignatureUrl { get; set; }// 🆕 Signature du directeur

        // Contact / Légal
        public string? LegalName { get; set; }           // 🆕 Raison sociale complète
        public string? RegistrationNumber { get; set; }  // 🆕 RCCM / SIRET
        public string? TaxNumber { get; set; }           // 🆕 NIU / TVA
        public string? Address { get; set; }             // 🆕 Adresse siège
        public string? City { get; set; }
        public string? Country { get; set; } = "Cameroun";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }

        // Direction
        public string? DirectorName { get; set; }        // 🆕 Nom du dirigeant (pour signatures)
        public string? DirectorTitle { get; set; }       // 🆕 Titre (PDG, Directeur Général...)

        // État
        public bool IsHolding { get; set; } = false;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    }
}