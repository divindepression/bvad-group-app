using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    public interface IInvoiceService
    {
        Task<List<InvoiceDto>> GetAllAsync(InvoiceFilters filters);
        Task<InvoiceDto?> GetByIdAsync(Guid id);
        Task<Invoice?> GetEntityAsync(Guid id);
        Task<InvoiceDto?> CreateAsync(CreateInvoiceDto dto, Guid? userId);
        Task<InvoiceDto?> UpdateAsync(Guid id, CreateInvoiceDto dto);
        Task<InvoiceDto?> IssueAsync(Guid id);
        Task<InvoiceDto?> CancelAsync(Guid id);
        Task RefreshStatusAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _context;
        private readonly IBillingNumberService _numbering;

        public InvoiceService(AppDbContext context, IBillingNumberService numbering)
        {
            _context = context;
            _numbering = numbering;
        }

        public async Task<List<InvoiceDto>> GetAllAsync(InvoiceFilters filters)
        {
            var query = _context.Invoices
                .Include(i => i.Company)
                .Include(i => i.Client)
                .Include(i => i.LineItems)
                .Include(i => i.Payments)
                .AsQueryable();

            if (filters.CompanyId.HasValue)
                query = query.Where(i => i.CompanyId == filters.CompanyId);

            if (filters.ClientId.HasValue)
                query = query.Where(i => i.ClientId == filters.ClientId);

            if (filters.Status.HasValue)
                query = query.Where(i => i.Status == filters.Status);

            var list = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();

            if (filters.Overdue == true)
                list = list.Where(i => i.IsOverdue).ToList();

            return list.Select(ToDto).ToList();
        }

        public async Task<InvoiceDto?> GetByIdAsync(Guid id)
        {
            var i = await GetEntityAsync(id);
            return i == null ? null : ToDto(i);
        }

        public async Task<Invoice?> GetEntityAsync(Guid id)
        {
            return await _context.Invoices
                .Include(i => i.Company)
                .Include(i => i.Client)
                .Include(i => i.LineItems.OrderBy(l => l.Order))
                .Include(i => i.Payments)
                    .ThenInclude(p => p.RecordedByUser)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<InvoiceDto?> CreateAsync(CreateInvoiceDto dto, Guid? userId)
        {
            var company = await _context.Companies.FindAsync(dto.CompanyId);
            var client = await _context.Clients.FindAsync(dto.ClientId);
            if (company == null || client == null) return null;

            var number = await _numbering.GenerateInvoiceNumberAsync(dto.CompanyId);

            var invoice = new Invoice
            {
                InvoiceNumber = number,
                CompanyId = dto.CompanyId,
                ClientId = dto.ClientId,
                IssueDate = dto.IssueDate.ToUniversalTime(),
                DueDate = dto.DueDate.ToUniversalTime(),
                Currency = dto.Currency,
                VatRate = dto.VatRate,
                Subject = dto.Subject,
                Notes = dto.Notes,
                PaymentTerms = dto.PaymentTerms,
                DiscountPercent = dto.DiscountPercent,
                Status = InvoiceStatus.Draft,
                CreatedByUserId = userId
            };

            int order = 1;
            foreach (var line in dto.LineItems)
            {
                invoice.LineItems.Add(new InvoiceLineItem
                {
                    Order = order++,
                    Description = line.Description,
                    Unit = line.Unit,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountPercent = line.DiscountPercent
                });
            }

            RecalculateTotals(invoice);
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            invoice.Company = company;
            invoice.Client = client;

            return ToDto(invoice);
        }

        public async Task<InvoiceDto?> UpdateAsync(Guid id, CreateInvoiceDto dto)
        {
            var invoice = await GetEntityAsync(id);
            if (invoice == null) return null;

            // Protection : impossible de modifier une facture émise/payée
            if (invoice.Status != InvoiceStatus.Draft)
                return null;

            invoice.CompanyId = dto.CompanyId;
            invoice.ClientId = dto.ClientId;
            invoice.IssueDate = dto.IssueDate.ToUniversalTime();
            invoice.DueDate = dto.DueDate.ToUniversalTime();
            invoice.Currency = dto.Currency;
            invoice.VatRate = dto.VatRate;
            invoice.Subject = dto.Subject;
            invoice.Notes = dto.Notes;
            invoice.PaymentTerms = dto.PaymentTerms;
            invoice.DiscountPercent = dto.DiscountPercent;
            invoice.UpdatedAt = DateTime.UtcNow;

            _context.InvoiceLineItems.RemoveRange(invoice.LineItems);
            invoice.LineItems.Clear();

            int order = 1;
            foreach (var line in dto.LineItems)
            {
                invoice.LineItems.Add(new InvoiceLineItem
                {
                    Order = order++,
                    Description = line.Description,
                    Unit = line.Unit,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountPercent = line.DiscountPercent
                });
            }

            RecalculateTotals(invoice);
            await _context.SaveChangesAsync();

            return ToDto(invoice);
        }

        public async Task<InvoiceDto?> IssueAsync(Guid id)
        {
            var invoice = await GetEntityAsync(id);
            if (invoice == null) return null;
            if (invoice.Status != InvoiceStatus.Draft) return null;

            invoice.Status = InvoiceStatus.Issued;
            invoice.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ToDto(invoice);
        }

        public async Task<InvoiceDto?> CancelAsync(Guid id)
        {
            var invoice = await GetEntityAsync(id);
            if (invoice == null) return null;
            if (invoice.Status == InvoiceStatus.Paid) return null;

            invoice.Status = InvoiceStatus.Cancelled;
            invoice.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ToDto(invoice);
        }

        /// <summary>Recalcule le statut selon les paiements.</summary>
        public async Task RefreshStatusAsync(Guid id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null || invoice.Status == InvoiceStatus.Cancelled) return;

            invoice.AmountPaid = invoice.Payments.Sum(p => p.Amount);

            if (invoice.AmountPaid >= invoice.TotalTTC)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidAt = DateTime.UtcNow;
            }
            else if (invoice.AmountPaid > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
                invoice.PaidAt = null;
            }
            else if (invoice.DueDate < DateTime.UtcNow && invoice.Status == InvoiceStatus.Issued)
            {
                invoice.Status = InvoiceStatus.Overdue;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return false;
            if (invoice.Status != InvoiceStatus.Draft)
                throw new InvalidOperationException("Impossible de supprimer une facture émise");

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        // ═══════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════
        private static void RecalculateTotals(Invoice invoice)
        {
            var totals = BillingCalculator.Calculate(
                invoice.LineItems.Select(l => (l.Quantity, l.UnitPrice, l.DiscountPercent)),
                invoice.DiscountPercent,
                invoice.VatRate
            );

            invoice.SubtotalHT = totals.SubtotalHT;
            invoice.DiscountAmount = totals.DiscountAmount;
            invoice.VatAmount = totals.VatAmount;
            invoice.TotalTTC = totals.TotalTTC;
        }

        private static InvoiceDto ToDto(Invoice i) => new(
            i.Id, i.InvoiceNumber,
            i.CompanyId, i.Company?.Name ?? "", i.Company?.Color ?? "#1e3a8a", i.Company?.Logo,
            i.ClientId, i.Client?.Name ?? "", i.Client?.DisplayName ?? "",
            i.IssueDate, i.DueDate, i.PaidAt,
            i.Currency, i.VatRate,
            i.Subject, i.Notes, i.PaymentTerms,
            i.SubtotalHT, i.VatAmount, i.TotalTTC,
            i.DiscountPercent, i.DiscountAmount,
            i.AmountPaid, i.AmountDue,
            i.Status.ToString(), i.IsOverdue, i.DaysOverdue,
            i.FromQuoteId,
            i.LineItems.OrderBy(l => l.Order).Select(l => new LineItemDto(
                l.Id, l.Order, l.Description, l.Unit,
                l.Quantity, l.UnitPrice, l.DiscountPercent, l.LineTotal
            )).ToList(),
            i.Payments.OrderByDescending(p => p.PaymentDate).Select(p => new PaymentDto(
                p.Id, p.PaymentNumber, p.InvoiceId,
                p.Amount, p.Currency,
                p.PaymentDate, p.Method.ToString(),
                p.MobileMoneyOperator?.ToString(),
                p.Reference, p.Notes,
                p.RecordedByUser?.FullName,
                p.CreatedAt
            )).ToList(),
            i.CreatedAt
        );
    }
}