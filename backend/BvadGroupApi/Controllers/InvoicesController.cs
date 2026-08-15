using System.Security.Claims;
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
    [Tags("🧾 Factures")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _service;

        public InvoicesController(IInvoiceService service)
        {
            _service = service;
        }

        private Guid? UserId
        {
            get
            {
                var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(s, out var g) ? g : null;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? companyId,
            [FromQuery] Guid? clientId,
            [FromQuery] InvoiceStatus? status,
            [FromQuery] bool? overdue)
        {
            return Ok(await _service.GetAllAsync(new InvoiceFilters(companyId, clientId, status, overdue)));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var i = await _service.GetByIdAsync(id);
            return i == null ? NotFound() : Ok(i);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager,Accountant")]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto)
        {
            if (dto.LineItems == null || dto.LineItems.Count == 0)
                return BadRequest(new { message = "Au moins une ligne de facture est requise" });

            var created = await _service.CreateAsync(dto, UserId);
            return created == null
                ? BadRequest(new { message = "Filiale ou client introuvable" })
                : CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager,Accountant")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateInvoiceDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated == null
                ? NotFound(new { message = "Facture introuvable ou non modifiable" })
                : Ok(updated);
        }

        [HttpPost("{id:guid}/issue")]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager,Accountant")]
        public async Task<IActionResult> Issue(Guid id)
        {
            var updated = await _service.IssueAsync(id);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpPost("{id:guid}/cancel")]
        [Authorize(Roles = "SuperAdmin,Admin,Director")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var updated = await _service.CancelAsync(id);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "SuperAdmin,Admin,Director")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                return (await _service.DeleteAsync(id)) ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:guid}/pdf")]
        public async Task<IActionResult> DownloadPdf(
    Guid id,
    [FromServices] IInvoicePdfService pdfService)
        {
            var invoice = await _service.GetEntityAsync(id);
            if (invoice == null) return NotFound();

            var pdfBytes = pdfService.Generate(invoice);
            var fileName = $"Facture_{invoice.InvoiceNumber}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

    }
}