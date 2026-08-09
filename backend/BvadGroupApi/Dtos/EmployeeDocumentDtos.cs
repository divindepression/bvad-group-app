using BvadGroupApi.Models;

namespace BvadGroupApi.Dtos
{
    public record EmployeeDocumentDto(
        Guid Id,
        Guid EmployeeId,
        string Type,
        string Title,
        string? Description,
        string FileName,
        string FileUrl,
        string? ContentType,
        long FileSize,
        DateTime? IssueDate,
        DateTime? ExpiryDate,
        bool IsVerified,
        bool IsExpired,
        bool IsExpiringSoon,
        DateTime CreatedAt
    );

    public record CreateDocumentMetadataDto(
        DocumentType Type,
        string Title,
        string? Description,
        DateTime? IssueDate,
        DateTime? ExpiryDate
    );

    public record UpdateDocumentDto(
        DocumentType Type,
        string Title,
        string? Description,
        DateTime? IssueDate,
        DateTime? ExpiryDate,
        bool IsVerified
    );
}