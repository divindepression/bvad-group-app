using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using BvadGroupApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("👨‍💼 Employés")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeesController(IEmployeeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? companyId,
            [FromQuery] string? search,
            [FromQuery] EmployeeStatus? status,
            [FromQuery] string? department)
        {
            var filters = new EmployeeFilters(companyId, search, status, department);
            var list = await _service.GetAllAsync(filters);
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var emp = await _service.GetByIdAsync(id);
            return emp == null ? NotFound() : Ok(emp);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            var created = await _service.CreateAsync(dto);
            if (created == null) return BadRequest(new { message = "Filiale introuvable" });
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        // ═══════════════════════════════════════════════════
        // 📸 Photo identité officielle
        // ═══════════════════════════════════════════════════
        [HttpPost("{id:guid}/identity-photo")]
        [RequestSizeLimit(5_000_000)]
        public async Task<IActionResult> UploadIdentityPhoto(
            Guid id,
            IFormFile file,
            [FromServices] Data.AppDbContext context,
            [FromServices] IFileStorageService storage)
        {
            var emp = await context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            if (!file.ContentType.StartsWith("image/"))
                return BadRequest(new { message = "Image requise" });

            var stored = await storage.SaveAsync(file, $"Employees/{id}/Identity");

            // Supprimer l'ancienne si elle existe
            if (!string.IsNullOrEmpty(emp.IdentityPhotoUrl))
                await storage.DeleteAsync(emp.IdentityPhotoUrl);

            emp.IdentityPhotoUrl = stored.RelativePath;
            emp.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return Ok(new { identityPhotoUrl = stored.RelativePath });
        }

        [HttpGet("{id:guid}/identity-photo")]
        [AllowAnonymous]
        public async Task<IActionResult> GetIdentityPhoto(
            Guid id,
            [FromServices] Data.AppDbContext context,
            [FromServices] IFileStorageService storage)
        {
            var emp = await context.Employees.FindAsync(id);
            if (emp == null || string.IsNullOrEmpty(emp.IdentityPhotoUrl))
                return NotFound();

            var bytes = await storage.ReadAsync(emp.IdentityPhotoUrl);
            if (bytes == null) return NotFound();

            var ext = Path.GetExtension(emp.IdentityPhotoUrl).ToLower();
            var contentType = ext switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            return File(bytes, contentType);
        }

        // ═══════════════════════════════════════════════════
        // 🎫 Badge PDF
        // ═══════════════════════════════════════════════════
        [HttpGet("{id:guid}/badge")]
        public async Task<IActionResult> DownloadBadge(
            Guid id,
            [FromServices] Data.AppDbContext context,
            [FromServices] IBadgePdfService badgeService)
        {
            var emp = await context.Employees
                .Include(e => e.Company)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emp == null) return NotFound();

            var pdfBytes = badgeService.GenerateBadge(emp);
            var fileName = $"Badge_{emp.EmployeeNumber ?? emp.Id.ToString()}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        // ═══════════════════════════════════════════════════
        // 🖋 Signature scannée employé
        // ═══════════════════════════════════════════════════
        [HttpPost("{id:guid}/signature")]
        [RequestSizeLimit(5_000_000)]
        public async Task<IActionResult> UploadSignature(
            Guid id,
            IFormFile file,
            [FromServices] Data.AppDbContext context,
            [FromServices] IFileStorageService storage)
        {
            var emp = await context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            if (!file.ContentType.StartsWith("image/"))
                return BadRequest(new { message = "Image requise (PNG transparent recommandé)" });

            var stored = await storage.SaveAsync(file, $"Employees/{id}/Signature");

            if (!string.IsNullOrEmpty(emp.SignatureUrl))
                await storage.DeleteAsync(emp.SignatureUrl);

            emp.SignatureUrl = stored.RelativePath;
            emp.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return Ok(new { signatureUrl = stored.RelativePath });
        }

        [HttpGet("{id:guid}/signature")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSignature(
            Guid id,
            [FromServices] Data.AppDbContext context,
            [FromServices] IFileStorageService storage)
        {
            var emp = await context.Employees.FindAsync(id);
            if (emp == null || string.IsNullOrEmpty(emp.SignatureUrl))
                return NotFound();

            var bytes = await storage.ReadAsync(emp.SignatureUrl);
            if (bytes == null) return NotFound();

            var ext = Path.GetExtension(emp.SignatureUrl).ToLower();
            var contentType = ext switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };

            return File(bytes, contentType);
        }

        // ═══════════════════════════════════════════════════
        // 📄 Fiche employé PDF officielle
        // ═══════════════════════════════════════════════════
        [HttpGet("{id:guid}/sheet-pdf")]
        public async Task<IActionResult> DownloadSheetPdf(
            Guid id,
            [FromServices] Data.AppDbContext context,
            [FromServices] IEmployeeSheetPdfService sheetService)
        {
            var emp = await context.Employees
                .Include(e => e.Company)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emp == null) return NotFound();

            var pdfBytes = sheetService.GenerateSheet(emp);
            var fileName = $"Fiche_{emp.EmployeeNumber ?? emp.Id.ToString()}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

    }
}