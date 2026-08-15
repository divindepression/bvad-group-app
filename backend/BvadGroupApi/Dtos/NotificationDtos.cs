using BvadGroupApi.Models;

namespace BvadGroupApi.Dtos
{
    public record NotificationDto(
        Guid Id,
        string Type,
        string Priority,
        string Title,
        string Message,
        string Icon,
        string Color,
        string? ActionUrl,
        Guid? RelatedEntityId,
        string? RelatedEntityType,
        bool IsRead,
        DateTime? ReadAt,
        DateTime CreatedAt
    );

    public record NotificationCountDto(
        int Total,
        int Unread
    );
}