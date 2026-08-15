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
    [Tags("📝 Devis")]
    public class QuotesController : ControllerBase
    {
        private readonly IQuoteService _service;

        public QuotesController(IQuoteService service)
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
            [FromQuery] QuoteStatus? status)
        {
            return Ok(await _service.GetAllAsync(new QuoteFilters(companyId, clientId, status)));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var q = await _service.GetByIdAsync(id);
            return q == null ? NotFound() : Ok(q);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager,Accountant")]
        public async Task<IActionResult> Create([FromBody] CreateQuoteDto dto)
        {
            if (dto.LineItems == null || dto.LineItems.Count == 0)
                return BadRequest(new { message = "Au moins une ligne de devis est requise" });

            var created = await _service.CreateAsync(dto, UserId);
            return created == null
                ? BadRequest(new { message = "Filiale ou client introuvable" })
                : CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager,Accountant")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateQuoteDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated == null
                ? NotFound(new { message = "Devis introuvable ou déjà converti" })
                : Ok(updated);
        }

        [HttpPost("{id:guid}/send")]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager,Accountant")]
        public async Task<IActionResult> MarkAsSent(Guid id)
        {
            var updated = await _service.UpdateStatusAsync(id, QuoteStatus.Sent);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpPost("{id:guid}/accept")]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager,Accountant")]
        public async Task<IActionResult> MarkAsAccepted(Guid id)
        {
            var updated = await _service.UpdateStatusAsync(id, QuoteStatus.Accepted);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpPost("{id:guid}/reject")]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager,Accountant")]
        public async Task<IActionResult> MarkAsRejected(Guid id, [FromBody] RejectQuoteDto dto)
        {
            var updated = await _service.UpdateStatusAsync(id, QuoteStatus.Rejected, dto.Reason);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpPost("{id:guid}/convert-to-invoice")]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager,Accountant")]
        public async Task<IActionResult> ConvertToInvoice(Guid id)
        {
            try
            {
                var invoice = await _service.ConvertToInvoiceAsync(id, UserId);
                return invoice == null ? NotFound() : Ok(invoice);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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
    [FromServices] IQuotePdfService pdfService)
        {
            var quote = await _service.GetEntityAsync(id);
            if (quote == null) return NotFound();

            var pdfBytes = pdfService.Generate(quote);
            var fileName = $"Devis_{quote.QuoteNumber}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

    }

    public record RejectQuoteDto(string? Reason);
}