namespace BvadGroupApi.Models
{
    /// <summary>
    /// Utilisateur du système BVAD
    /// </summary>
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? PhotoUrl { get; set; }

        /// <summary>Rôle global de l'utilisateur</summary>
        public UserRole Role { get; set; } = UserRole.Employee;

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation : liste des filiales auxquelles l'utilisateur a accès
        public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();

        /// <summary>Nom complet calculé</summary>
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}