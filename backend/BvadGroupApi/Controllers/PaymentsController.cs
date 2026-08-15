using System.Security.Claims;
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
    [Tags("💳 Paiements")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentsController(IPaymentService service)
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

        [HttpGet("invoice/{invoiceId:guid}")]
        public async Task<IActionResult> GetByInvoice(Guid invoiceId)
        {
            return Ok(await _service.GetByInvoiceAsync(invoiceId));
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager,Accountant")]
        public async Task<IActionResult> Record([FromBody] CreatePaymentDto dto)
        {
            try
            {
                var payment = await _service.RecordAsync(dto, UserId);
                return payment == null ? BadRequest(new { message = "Facture introuvable" }) : Ok(payment);
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
            return (await _service.DeleteAsync(id)) ? NoContent() : NotFound();
        }

        [HttpGet("{id:guid}/receipt")]
        public async Task<IActionResult> DownloadReceipt(
    Guid id,
    [FromServices] IReceiptPdfService pdfService,
    [FromServices] Data.AppDbContext context)
        {
            var payment = await context.Payments
                .Include(p => p.Invoice).ThenInclude(i => i!.Company)
                .Include(p => p.Invoice).ThenInclude(i => i!.Client)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return NotFound();

            var pdfBytes = pdfService.Generate(payment);
            var fileName = $"Recu_{payment.PaymentNumber}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

    }
}