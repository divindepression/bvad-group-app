using System.Security.Claims;
using System.Text.Json;
using BvadGroupApi.Dtos;
using BvadGroupApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BvadGroupApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/employees/{employeeId:guid}/documents")]
    [Tags("📎 Documents employés")]
    public class EmployeeDocumentsController : ControllerBase
    {
        private readonly IEmployeeDocumentService _service;
        private readonly IFileStorageService _storage;

        public EmployeeDocumentsController(
            IEmployeeDocumentService service,
            IFileStorageService storage)
        {
            _service = service;
            _storage = storage;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid employeeId)
        {
            var docs = await _service.GetByEmployeeAsync(employeeId);
            return Ok(docs);
        }

        [HttpPost]
        [RequestSizeLimit(20_000_000)]  // 20 MB max
        public async Task<IActionResult> Upload(
            Guid employeeId,
            IFormFile file,
            [FromForm] string metadata)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Fichier vide" });

            var meta = JsonSerializer.Deserialize<CreateDocumentMetadataDto>(
                metadata,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (meta == null)
                return BadRequest(new { message = "Métadonnées invalides" });

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? userId = Guid.TryParse(userIdStr, out var g) ? g : null;

            var doc = await _service.UploadAsync(employeeId, file, meta, userId);
            if (doc == null) return NotFound(new { message = "Employé introuvable" });

            return Ok(doc);
        }

        [HttpGet("{id:guid}/download")]
        [AllowAnonymous]
        public async Task<IActionResult> Download(Guid employeeId, Guid id)
        {
            var doc = await _service.GetEntityAsync(id);
            if (doc == null || doc.EmployeeId != employeeId) return NotFound();

            var bytes = await _storage.ReadAsync(doc.FileUrl);
            if (bytes == null) return NotFound();

            return File(bytes, doc.ContentType ?? "application/octet-stream", doc.FileName);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid employeeId, Guid id, [FromBody] UpdateDocumentDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid employeeId, Guid id)
        {
            return (await _service.DeleteAsync(id)) ? NoContent() : NotFound();
        }
    }
}