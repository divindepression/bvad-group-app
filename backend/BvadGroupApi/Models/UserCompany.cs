namespace BvadGroupApi.Models
{
    /// <summary>
    /// Lien entre un utilisateur et une filiale.
    /// Permet à un user d'accéder à plusieurs filiales.
    /// </summary>
    public class UserCompany
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        /// <summary>Rôle spécifique dans cette filiale (peut différer du rôle global)</summary>
        public UserRole? CompanyRole { get; set; }

        /// <summary>True si c'est la filiale par défaut (celle qu'on sélectionne au login)</summary>
        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}