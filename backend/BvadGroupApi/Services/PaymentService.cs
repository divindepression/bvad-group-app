using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    public interface IPaymentService
    {
        Task<PaymentDto?> RecordAsync(CreatePaymentDto dto, Guid? userId);
        Task<List<PaymentDto>> GetByInvoiceAsync(Guid invoiceId);
        Task<bool> DeleteAsync(Guid id);
    }

    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IBillingNumberService _numbering;
        private readonly IInvoiceService _invoiceService;

        public PaymentService(AppDbContext context, IBillingNumberService numbering, IInvoiceService invoiceService)
        {
            _context = context;
            _numbering = numbering;
            _invoiceService = invoiceService;
        }

        public async Task<PaymentDto?> RecordAsync(CreatePaymentDto dto, Guid? userId)
        {
            var invoice = await _context.Invoices.FindAsync(dto.InvoiceId);
            if (invoice == null) return null;

            if (invoice.Status == InvoiceStatus.Draft)
                throw new InvalidOperationException("Impossible de payer une facture en brouillon. Émettez-la d'abord.");

            if (invoice.Status == InvoiceStatus.Cancelled)
                throw new InvalidOperationException("Impossible de payer une facture annulée");

            if (dto.Amount <= 0)
                throw new InvalidOperationException("Le montant doit être positif");

            if (dto.Amount > invoice.AmountDue)
                throw new InvalidOperationException($"Le montant dépasse le solde dû ({invoice.AmountDue} {invoice.Currency})");

            var payment = new Payment
            {
                PaymentNumber = await _numbering.GeneratePaymentNumberAsync(),
                InvoiceId = dto.InvoiceId,
                Amount = dto.Amount,
                Currency = dto.Currency,
                PaymentDate = dto.PaymentDate.ToUniversalTime(),
                Method = dto.Method,
                MobileMoneyOperator = dto.MobileMoneyOperator,
                Reference = dto.Reference,
                Notes = dto.Notes,
                RecordedByUserId = userId
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Recalculer le statut de la facture
            await _invoiceService.RefreshStatusAsync(dto.InvoiceId);

            var withUser = await _context.Payments
                .Include(p => p.RecordedByUser)
                .FirstAsync(p => p.Id == payment.Id);

            return ToDto(withUser);
        }

        public async Task<List<PaymentDto>> GetByInvoiceAsync(Guid invoiceId)
        {
            var payments = await _context.Payments
                .Include(p => p.RecordedByUser)
                .Where(p => p.InvoiceId == invoiceId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return payments.Select(ToDto).ToList();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return false;

            var invoiceId = payment.InvoiceId;
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            // Recalculer la facture
            await _invoiceService.RefreshStatusAsync(invoiceId);
            return true;
        }

        private static PaymentDto ToDto(Payment p) => new(
            p.Id, p.PaymentNumber, p.InvoiceId,
            p.Amount, p.Currency,
            p.PaymentDate, p.Method.ToString(),
            p.MobileMoneyOperator?.ToString(),
            p.Reference, p.Notes,
            p.RecordedByUser?.FullName,
            p.CreatedAt
        );
    }
}