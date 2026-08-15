namespace BvadGroupApi.Models
{
    /// <summary>
    /// Client de BVAD GROUP (particulier ou entreprise).
    /// Peut être facturé par n'importe quelle filiale.
    /// </summary>
    public class Client
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // 🆔 Identifiant interne
        public string? ClientCode { get; set; }  // ex: CLI-2026-0001

        // 👤 Type
        public ClientType Type { get; set; } = ClientType.Individual;

        // 📇 Identité
        public string Name { get; set; } = string.Empty;  // Nom OU raison sociale
        public string? ContactPerson { get; set; }        // Personne de contact si entreprise
        public string? Position { get; set; }             // Poste de la personne de contact

        // 🏛 Entreprise (si Type = Company)
        public string? LegalForm { get; set; }            // SARL, SA, SASU...
        public string? RegistrationNumber { get; set; }   // RCCM
        public string? TaxNumber { get; set; }            // NIU
        public decimal? Capital { get; set; }

        // 📞 Contact
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? SecondaryPhone { get; set; }
        public string? Website { get; set; }

        // 📍 Adresse
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; } = "Congo";
        public string? PostalCode { get; set; }

        // 💰 Facturation
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;

        // 🕒 Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 🧮 Calculé
        public string DisplayName => Type == ClientType.Company
            ? (Name + (LegalForm != null ? $" {LegalForm}" : ""))
            : Name;
    }
}