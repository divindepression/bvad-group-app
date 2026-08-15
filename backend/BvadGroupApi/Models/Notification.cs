namespace BvadGroupApi.Models
{
    /// <summary>
    /// Notification stockée en base pour historique et badge de compteur.
    /// </summary>
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Utilisateur destinataire</summary>
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        /// <summary>Icône emoji</summary>
        public string Icon { get; set; } = "🔔";

        /// <summary>Couleur (hex)</summary>
        public string Color { get; set; } = "#3b82f6";

        /// <summary>Route Angular à ouvrir au clic (ex: "/leave-approvals")</summary>
        public string? ActionUrl { get; set; }

        /// <summary>ID de l'entité liée (ex: LeaveRequest.Id)</summary>
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }  // "LeaveRequest", "Contract", etc.

        /// <summary>Filiale concernée (pour filtrage)</summary>
        public Guid? CompanyId { get; set; }

        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}