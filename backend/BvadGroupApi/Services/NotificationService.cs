using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Hubs;
using BvadGroupApi.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    public interface INotificationService
    {
        Task SendToUserAsync(Guid userId, NotificationType type, string title, string message,
            string? actionUrl = null, Guid? relatedEntityId = null, string? relatedEntityType = null,
            Guid? companyId = null, string? icon = null, string? color = null,
            NotificationPriority priority = NotificationPriority.Normal);

        Task SendToUsersAsync(List<Guid> userIds, NotificationType type, string title, string message,
            string? actionUrl = null, Guid? relatedEntityId = null, string? relatedEntityType = null,
            Guid? companyId = null, string? icon = null, string? color = null,
            NotificationPriority priority = NotificationPriority.Normal);

        Task SendToCompanyManagersAsync(Guid companyId, NotificationType type, string title, string message,
            string? actionUrl = null, Guid? relatedEntityId = null, string? relatedEntityType = null,
            string? icon = null, string? color = null,
            NotificationPriority priority = NotificationPriority.Normal);

        Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int take = 50);
        Task<NotificationCountDto> GetCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid userId, Guid notificationId);
        Task MarkAllAsReadAsync(Guid userId);
        Task DeleteAsync(Guid userId, Guid notificationId);
    }

    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            AppDbContext context,
            IHubContext<NotificationHub> hub,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _hub = hub;
            _logger = logger;
        }

        // ═══════════════════════════════════════
        // Envoi à UN utilisateur
        // ═══════════════════════════════════════
        public async Task SendToUserAsync(Guid userId, NotificationType type, string title, string message,
            string? actionUrl = null, Guid? relatedEntityId = null, string? relatedEntityType = null,
            Guid? companyId = null, string? icon = null, string? color = null,
            NotificationPriority priority = NotificationPriority.Normal)
        {
            var notif = new Notification
            {
                UserId = userId,
                Type = type,
                Priority = priority,
                Title = title,
                Message = message,
                Icon = icon ?? GetDefaultIcon(type),
                Color = color ?? GetDefaultColor(type),
                ActionUrl = actionUrl,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                CompanyId = companyId
            };

            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();

            // Push SignalR temps réel
            var dto = ToDto(notif);
            await _hub.Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", dto);

            _logger.LogInformation("🔔 Notification envoyée à {UserId} : {Title}", userId, title);
        }

        // ═══════════════════════════════════════
        // Envoi à PLUSIEURS utilisateurs
        // ═══════════════════════════════════════
        public async Task SendToUsersAsync(List<Guid> userIds, NotificationType type, string title, string message,
            string? actionUrl = null, Guid? relatedEntityId = null, string? relatedEntityType = null,
            Guid? companyId = null, string? icon = null, string? color = null,
            NotificationPriority priority = NotificationPriority.Normal)
        {
            var notifs = userIds.Select(userId => new Notification
            {
                UserId = userId,
                Type = type,
                Priority = priority,
                Title = title,
                Message = message,
                Icon = icon ?? GetDefaultIcon(type),
                Color = color ?? GetDefaultColor(type),
                ActionUrl = actionUrl,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                CompanyId = companyId
            }).ToList();

            _context.Notifications.AddRange(notifs);
            await _context.SaveChangesAsync();

            // Push chacun
            foreach (var notif in notifs)
            {
                var dto = ToDto(notif);
                await _hub.Clients.Group($"user-{notif.UserId}").SendAsync("ReceiveNotification", dto);
            }

            _logger.LogInformation("🔔 {Count} notifications envoyées : {Title}", userIds.Count, title);
        }

        // ═══════════════════════════════════════
        // Envoi aux managers/HR/Director d'une filiale
        // ═══════════════════════════════════════
        public async Task SendToCompanyManagersAsync(Guid companyId, NotificationType type, string title, string message,
            string? actionUrl = null, Guid? relatedEntityId = null, string? relatedEntityType = null,
            string? icon = null, string? color = null,
            NotificationPriority priority = NotificationPriority.Normal)
        {
            // Récupérer les users ayant un rôle manager+ dans cette filiale
            var managerRoles = new[] { UserRole.SuperAdmin, UserRole.Admin, UserRole.Director, UserRole.HR, UserRole.Manager };

            var userIds = await _context.UserCompanies
                .Where(uc => uc.CompanyId == companyId
                          && uc.CompanyRole.HasValue
                          && managerRoles.Contains(uc.CompanyRole.Value))
                .Select(uc => uc.UserId)
                .Distinct()
                .ToListAsync();

            // Ajouter tous les SuperAdmin globaux
            var superAdmins = await _context.Users
                .Where(u => u.Role == UserRole.SuperAdmin)
                .Select(u => u.Id)
                .ToListAsync();

            userIds.AddRange(superAdmins);
            userIds = userIds.Distinct().ToList();

            if (userIds.Count == 0)
            {
                _logger.LogWarning("⚠ Aucun manager trouvé pour filiale {CompanyId}", companyId);
                return;
            }

            await SendToUsersAsync(userIds, type, title, message, actionUrl,
                relatedEntityId, relatedEntityType, companyId, icon, color, priority);
        }

        // ═══════════════════════════════════════
        // Consultation
        // ═══════════════════════════════════════
        public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int take = 50)
        {
            var notifs = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync();

            return notifs.Select(ToDto).ToList();
        }

        public async Task<NotificationCountDto> GetCountAsync(Guid userId)
        {
            var total = await _context.Notifications.CountAsync(n => n.UserId == userId);
            var unread = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
            return new NotificationCountDto(total, unread);
        }

        public async Task MarkAsReadAsync(Guid userId, Guid notificationId)
        {
            var notif = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notif == null || notif.IsRead) return;

            notif.IsRead = true;
            notif.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _hub.Clients.Group($"user-{userId}").SendAsync("NotificationRead", notificationId);
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var notifs = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifs)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            await _hub.Clients.Group($"user-{userId}").SendAsync("AllNotificationsRead");
        }

        public async Task DeleteAsync(Guid userId, Guid notificationId)
        {
            var notif = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notif == null) return;

            _context.Notifications.Remove(notif);
            await _context.SaveChangesAsync();

            await _hub.Clients.Group($"user-{userId}").SendAsync("NotificationDeleted", notificationId);
        }

        // ═══════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════
        private static NotificationDto ToDto(Notification n) =>
            new(n.Id, n.Type.ToString(), n.Priority.ToString(), n.Title, n.Message,
                n.Icon, n.Color, n.ActionUrl, n.RelatedEntityId, n.RelatedEntityType,
                n.IsRead, n.ReadAt, n.CreatedAt);

        private static string GetDefaultIcon(NotificationType type) => type switch
        {
            NotificationType.LeaveRequestSubmitted => "🏖",
            NotificationType.LeaveRequestApproved => "✅",
            NotificationType.LeaveRequestRejected => "❌",
            NotificationType.LeaveRequestCancelled => "🚫",
            NotificationType.ContractExpiringSoon => "⚠",
            NotificationType.ContractExpired => "🚨",
            NotificationType.ContractCreated => "📄",
            NotificationType.ContractRenewed => "🔄",
            NotificationType.EmployeeHired => "👋",
            NotificationType.EmployeeLeft => "👋",
            NotificationType.EmployeeBirthday => "🎂",
            NotificationType.SystemAlert => "🚨",
            _ => "🔔"
        };

        private static string GetDefaultColor(NotificationType type) => type switch
        {
            NotificationType.LeaveRequestSubmitted => "#3b82f6",
            NotificationType.LeaveRequestApproved => "#22c55e",
            NotificationType.LeaveRequestRejected => "#ef4444",
            NotificationType.LeaveRequestCancelled => "#94a3b8",
            NotificationType.ContractExpiringSoon => "#f59e0b",
            NotificationType.ContractExpired => "#ef4444",
            NotificationType.ContractCreated => "#8b5cf6",
            NotificationType.ContractRenewed => "#06b6d4",
            NotificationType.EmployeeHired => "#22c55e",
            NotificationType.EmployeeLeft => "#94a3b8",
            NotificationType.EmployeeBirthday => "#ec4899",
            NotificationType.SystemAlert => "#dc2626",
            _ => "#3b82f6"
        };
    }
}