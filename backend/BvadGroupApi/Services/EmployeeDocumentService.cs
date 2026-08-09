using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    public interface IEmployeeDocumentService
    {
        Task<List<EmployeeDocumentDto>> GetByEmployeeAsync(Guid employeeId);
        Task<EmployeeDocument?> GetEntityAsync(Guid id);
        Task<EmployeeDocumentDto?> UploadAsync(
            Guid employeeId,
            IFormFile file,
            CreateDocumentMetadataDto metadata,
            Guid? uploadedByUserId);
        Task<EmployeeDocumentDto?> UpdateAsync(Guid id, UpdateDocumentDto dto);
        Task<bool> DeleteAsync(Guid id);
    }

    public class EmployeeDocumentService : IEmployeeDocumentService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _storage;

        public EmployeeDocumentService(AppDbContext context, IFileStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        public async Task<List<EmployeeDocumentDto>> GetByEmployeeAsync(Guid employeeId)
        {
            var docs = await _context.EmployeeDocuments
                .Where(d => d.EmployeeId == employeeId)
                .OrderBy(d => d.Type)
                .ThenByDescending(d => d.CreatedAt)
                .ToListAsync();

            return docs.Select(ToDto).ToList();
        }

        public async Task<EmployeeDocument?> GetEntityAsync(Guid id)
        {
            return await _context.EmployeeDocuments.FindAsync(id);
        }

        public async Task<EmployeeDocumentDto?> UploadAsync(
            Guid employeeId,
            IFormFile file,
            CreateDocumentMetadataDto metadata,
            Guid? uploadedByUserId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return null;

            // Sauver le fichier
            var stored = await _storage.SaveAsync(file, $"Employees/{employeeId}/Documents");

            // Enregistrer en base
            var doc = new EmployeeDocument
            {
                EmployeeId = employeeId,
                Type = metadata.Type,
                Title = metadata.Title,
                Description = metadata.Description,
                FileName = stored.FileName,
                FileUrl = stored.RelativePath,
                ContentType = stored.ContentType,
                FileSize = stored.Size,
                IssueDate = metadata.IssueDate?.ToUniversalTime(),
                ExpiryDate = metadata.ExpiryDate?.ToUniversalTime(),
                UploadedById = uploadedByUserId
            };

            _context.EmployeeDocuments.Add(doc);
            await _context.SaveChangesAsync();

            return ToDto(doc);
        }

        public async Task<EmployeeDocumentDto?> UpdateAsync(Guid id, UpdateDocumentDto dto)
        {
            var doc = await _context.EmployeeDocuments.FindAsync(id);
            if (doc == null) return null;

            doc.Type = dto.Type;
            doc.Title = dto.Title;
            doc.Description = dto.Description;
            doc.IssueDate = dto.IssueDate?.ToUniversalTime();
            doc.ExpiryDate = dto.ExpiryDate?.ToUniversalTime();
            doc.IsVerified = dto.IsVerified;

            await _context.SaveChangesAsync();
            return ToDto(doc);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var doc = await _context.EmployeeDocuments.FindAsync(id);
            if (doc == null) return false;

            // Supprimer le fichier physique
            await _storage.DeleteAsync(doc.FileUrl);

            _context.EmployeeDocuments.Remove(doc);
            await _context.SaveChangesAsync();
            return true;
        }

        private static EmployeeDocumentDto ToDto(EmployeeDocument d) =>
            new(
                d.Id,
                d.EmployeeId,
                d.Type.ToString(),
                d.Title,
                d.Description,
                d.FileName,
                d.FileUrl,
                d.ContentType,
                d.FileSize,
                d.IssueDate,
                d.ExpiryDate,
                d.IsVerified,
                d.IsExpired,
                d.IsExpiringSoon,
                d.CreatedAt
            );
    }
}