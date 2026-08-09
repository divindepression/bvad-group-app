using System.Security.Claims;
using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using BvadGroupApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BvadGroupApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("📄 Contrats")]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _service;
        private readonly IContractPdfService _pdfService;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ContractsController(
            IContractService service,
            IContractPdfService pdfService,
            AppDbContext context,
            IWebHostEnvironment env)
        {
            _service = service;
            _pdfService = pdfService;
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? companyId,
            [FromQuery] Guid? employeeId,
            [FromQuery] ContractStatus? status,
            [FromQuery] ContractType? type,
            [FromQuery] bool? expiringSoon)
        {
            var filters = new ContractFilters(companyId, employeeId, status, type, expiringSoon);
            return Ok(await _service.GetAllAsync(filters));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var c = await _service.GetByIdAsync(id);
            return c == null ? NotFound() : Ok(c);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateContractDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? userId = Guid.TryParse(userIdStr, out var g) ? g : null;

            var created = await _service.CreateAsync(dto, userId);
            if (created == null) return BadRequest(new { message = "Employé introuvable" });
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContractDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return (await _service.DeleteAsync(id)) ? NoContent() : NotFound();
        }

        // ================================================
        // 📥 Télécharger PDF généré automatiquement
        // ================================================
        [HttpGet("{id:guid}/pdf")]
        public async Task<IActionResult> DownloadPdf(Guid id)
        {
            var contract = await _service.GetContractEntityAsync(id);
            if (contract == null) return NotFound();

            var pdfBytes = _pdfService.GenerateContractPdf(contract);
            var fileName = $"Contrat_{contract.ContractNumber}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        // ================================================
        // 📤 Upload PDF signé
        // ================================================
        [HttpPost("{id:guid}/upload")]
        [RequestSizeLimit(10_000_000)]  // 10 MB max
        public async Task<IActionResult> UploadSignedContract(Guid id, IFormFile file)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Fichier vide" });

            if (!file.ContentType.Contains("pdf"))
                return BadRequest(new { message = "Format PDF requis" });

            // Créer dossier
            var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads", "Contracts");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{contract.ContractNumber}_{Guid.NewGuid().ToString("N")[..8]}.pdf";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            contract.DocumentUrl = $"Uploads/Contracts/{fileName}";
            contract.DocumentFileName = file.FileName;
            contract.DocumentSize = file.Length;
            contract.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Contrat signé uploadé",
                documentUrl = contract.DocumentUrl,
                size = file.Length
            });
        }

        // ================================================
        // 📄 Télécharger le PDF signé
        // ================================================
        [HttpGet("{id:guid}/signed-document")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadSignedDocument(Guid id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.DocumentUrl))
                return NotFound();

            var fullPath = Path.Combine(_env.ContentRootPath, contract.DocumentUrl);
            if (!System.IO.File.Exists(fullPath)) return NotFound();

            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, "application/pdf", contract.DocumentFileName ?? "contrat.pdf");
        }
    }
}