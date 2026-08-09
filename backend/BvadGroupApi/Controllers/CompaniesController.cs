using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("🏢 Filiales")]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _storage;

        public CompaniesController(AppDbContext context, IFileStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Companies
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return Ok(list.Select(ToDto));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var c = await _context.Companies.FindAsync(id);
            return c == null ? NotFound() : Ok(ToDto(c));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyDto dto)
        {
            var c = await _context.Companies.FindAsync(id);
            if (c == null) return NotFound();

            c.Name = dto.Name;
            c.LegalName = dto.LegalName;
            c.Description = dto.Description;
            c.Slogan = dto.Slogan;
            c.Color = dto.Color;
            c.RegistrationNumber = dto.RegistrationNumber;
            c.TaxNumber = dto.TaxNumber;
            c.Address = dto.Address;
            c.City = dto.City;
            c.Country = dto.Country;
            c.Phone = dto.Phone;
            c.Email = dto.Email;
            c.Website = dto.Website;
            c.DirectorName = dto.DirectorName;
            c.DirectorTitle = dto.DirectorTitle;
            c.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ToDto(c));
        }

        // ═══════════════════════════════════════
        // 🖼 UPLOAD LOGO / CACHET / SIGNATURE
        // ═══════════════════════════════════════
        [HttpPost("{id:guid}/logo")]
        [RequestSizeLimit(5_000_000)]
        public async Task<IActionResult> UploadLogo(Guid id, IFormFile file)
            => await UploadAssetAsync(id, file, "Logo");

        [HttpPost("{id:guid}/stamp")]
        [RequestSizeLimit(5_000_000)]
        public async Task<IActionResult> UploadStamp(Guid id, IFormFile file)
            => await UploadAssetAsync(id, file, "Stamp");

        [HttpPost("{id:guid}/director-signature")]
        [RequestSizeLimit(5_000_000)]
        public async Task<IActionResult> UploadDirectorSignature(Guid id, IFormFile file)
            => await UploadAssetAsync(id, file, "DirectorSignature");

        private async Task<IActionResult> UploadAssetAsync(Guid id, IFormFile file, string assetType)
        {
            var c = await _context.Companies.FindAsync(id);
            if (c == null) return NotFound();

            if (!file.ContentType.StartsWith("image/"))
                return BadRequest(new { message = "Image requise (PNG recommandé pour cachet/signature)" });

            var stored = await _storage.SaveAsync(file, $"Companies/{id}/{assetType}");

            // Supprimer ancien
            string? oldUrl = assetType switch
            {
                "Logo" => c.LogoUrl,
                "Stamp" => c.StampUrl,
                "DirectorSignature" => c.DirectorSignatureUrl,
                _ => null
            };
            if (!string.IsNullOrEmpty(oldUrl))
                await _storage.DeleteAsync(oldUrl);

            // Assigner nouveau
            switch (assetType)
            {
                case "Logo": c.LogoUrl = stored.RelativePath; break;
                case "Stamp": c.StampUrl = stored.RelativePath; break;
                case "DirectorSignature": c.DirectorSignatureUrl = stored.RelativePath; break;
            }

            c.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { url = stored.RelativePath });
        }

        // ═══════════════════════════════════════
        // 📥 DOWNLOAD assets
        // ═══════════════════════════════════════
        [HttpGet("{id:guid}/logo")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLogo(Guid id)
            => await GetAssetAsync(id, "Logo");

        [HttpGet("{id:guid}/stamp")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStamp(Guid id)
            => await GetAssetAsync(id, "Stamp");

        [HttpGet("{id:guid}/director-signature")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDirectorSignature(Guid id)
            => await GetAssetAsync(id, "DirectorSignature");

        private async Task<IActionResult> GetAssetAsync(Guid id, string assetType)
        {
            var c = await _context.Companies.FindAsync(id);
            if (c == null) return NotFound();

            string? url = assetType switch
            {
                "Logo" => c.LogoUrl,
                "Stamp" => c.StampUrl,
                "DirectorSignature" => c.DirectorSignatureUrl,
                _ => null
            };

            if (string.IsNullOrEmpty(url)) return NotFound();

            var bytes = await _storage.ReadAsync(url);
            if (bytes == null) return NotFound();

            var ext = Path.GetExtension(url).ToLower();
            var contentType = ext switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };

            return File(bytes, contentType);
        }

        // ═══════════════════════════════════════
        // ➕ CRÉATION
        // ═══════════════════════════════════════
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)
        {
            // Validation code
            if (string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest(new { message = "Le code est obligatoire" });

            var normalizedCode = dto.Code.Trim().ToUpperInvariant().Replace(" ", "_");

            // Code unique ?
            if (await _context.Companies.AnyAsync(c => c.Code == normalizedCode))
                return BadRequest(new { message = $"Le code '{normalizedCode}' existe déjà" });

            // Une seule holding possible ?
            if (dto.IsHolding && await _context.Companies.AnyAsync(c => c.IsHolding))
                return BadRequest(new { message = "Une holding existe déjà (une seule autorisée)" });

            var company = new Models.Company
            {
                Code = normalizedCode,
                Name = dto.Name,
                LegalName = dto.LegalName,
                Description = dto.Description,
                Slogan = dto.Slogan,
                Color = dto.Color,
                Logo = dto.Logo ?? "🏢",
                IsHolding = dto.IsHolding,
                DisplayOrder = dto.DisplayOrder,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country ?? "Cameroun",
                Phone = dto.Phone,
                Email = dto.Email,
                Website = dto.Website,
                RegistrationNumber = dto.RegistrationNumber,
                TaxNumber = dto.TaxNumber,
                DirectorName = dto.DirectorName,
                DirectorTitle = dto.DirectorTitle,
                IsActive = true
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // 🔗 Donner accès automatiquement à tous les SuperAdmins
            var superAdmins = await _context.Users
                .Where(u => u.Role == Models.UserRole.SuperAdmin)
                .ToListAsync();

            foreach (var admin in superAdmins)
            {
                _context.UserCompanies.Add(new Models.UserCompany
                {
                    UserId = admin.Id,
                    CompanyId = company.Id,
                    CompanyRole = Models.UserRole.SuperAdmin,
                    IsDefault = false
                });
            }
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = company.Id }, ToDto(company));
        }

        // ═══════════════════════════════════════
        // 🗑 SUPPRESSION
        // ═══════════════════════════════════════
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            // Protection holding
            if (company.IsHolding)
                return BadRequest(new { message = "Impossible de supprimer la holding" });

            // Protection employés
            var employeeCount = await _context.Employees.CountAsync(e => e.CompanyId == id);
            if (employeeCount > 0)
                return BadRequest(new { message = $"Impossible : {employeeCount} employé(s) rattaché(s) à cette filiale" });

            // Protection contrats
            var contractCount = await _context.Contracts.CountAsync(c => c.CompanyId == id);
            if (contractCount > 0)
                return BadRequest(new { message = $"Impossible : {contractCount} contrat(s) rattaché(s) à cette filiale" });

            // Supprimer les assets uploadés
            if (!string.IsNullOrEmpty(company.LogoUrl))
                await _storage.DeleteAsync(company.LogoUrl);
            if (!string.IsNullOrEmpty(company.StampUrl))
                await _storage.DeleteAsync(company.StampUrl);
            if (!string.IsNullOrEmpty(company.DirectorSignatureUrl))
                await _storage.DeleteAsync(company.DirectorSignatureUrl);

            // Supprimer les UserCompanies liés (cascade)
            var userCompanies = await _context.UserCompanies
                .Where(uc => uc.CompanyId == id)
                .ToListAsync();
            _context.UserCompanies.RemoveRange(userCompanies);

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ═══════════════════════════════════════
        // Helper
        // ═══════════════════════════════════════
        private static CompanyDto ToDto(Models.Company c) =>
            new(
                c.Id, c.Code, c.Name, c.LegalName, c.Description, c.Slogan,
                c.Color, c.Logo, c.LogoUrl, c.StampUrl, c.DirectorSignatureUrl,
                c.RegistrationNumber, c.TaxNumber,
                c.Address, c.City, c.Country,
                c.Phone, c.Email, c.Website,
                c.DirectorName, c.DirectorTitle,
                c.IsHolding, c.DisplayOrder, c.IsActive
            );
    }
}