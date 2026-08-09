namespace BvadGroupApi.Models
{
    /// <summary>
    /// Représente une entité du groupe BVAD (Holding ou Filiale)
    /// </summary>
    public class Company
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Code unique (ex: BVAD_GROUP, BVAD_AGRO)</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Nom complet (ex: "BVAD Agro")</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Description courte</summary>
        public string? Description { get; set; }

        /// <summary>Couleur d'identité (hex ex: #16a34a)</summary>
        public string Color { get; set; } = "#1e3a8a";

        /// <summary>Emoji ou URL du logo</summary>
        public string? Logo { get; set; }

        /// <summary>True si c'est la holding (BVAD GROUP mère)</summary>
        public bool IsHolding { get; set; } = false;

        /// <summary>Ordre d'affichage</summary>
        public int DisplayOrder { get; set; }

        /// <summary>Active / désactivée</summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation : liste des utilisateurs qui ont accès à cette filiale
        public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    }
}