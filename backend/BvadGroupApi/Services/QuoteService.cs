using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    public interface IQuoteService
    {
        Task<List<QuoteDto>> GetAllAsync(QuoteFilters filters);
        Task<QuoteDto?> GetByIdAsync(Guid id);
        Task<Quote?> GetEntityAsync(Guid id);
        Task<QuoteDto?> CreateAsync(CreateQuoteDto dto, Guid? userId);
        Task<QuoteDto?> UpdateAsync(Guid id, CreateQuoteDto dto);
        Task<QuoteDto?> UpdateStatusAsync(Guid id, QuoteStatus status, string? rejectionReason = null);
        Task<InvoiceDto?> ConvertToInvoiceAsync(Guid quoteId, Guid? userId);
        Task<bool> DeleteAsync(Guid id);
    }

    public class QuoteService : IQuoteService
    {
        private readonly AppDbContext _context;
        private readonly IBillingNumberService _numbering;
        private readonly IInvoiceService _invoiceService;

        public QuoteService(AppDbContext context, IBillingNumberService numbering, IInvoiceService invoiceService)
        {
            _context = context;
            _numbering = numbering;
            _invoiceService = invoiceService;
        }

        public async Task<List<QuoteDto>> GetAllAsync(QuoteFilters filters)
        {
            var query = _context.Quotes
                .Include(q => q.Company)
                .Include(q => q.Client)
                .Include(q => q.LineItems)
                .AsQueryable();

            if (filters.CompanyId.HasValue)
                query = query.Where(q => q.CompanyId == filters.CompanyId);

            if (filters.ClientId.HasValue)
                query = query.Where(q => q.ClientId == filters.ClientId);

            if (filters.Status.HasValue)
                query = query.Where(q => q.Status == filters.Status);

            var list = await query.OrderByDescending(q => q.CreatedAt).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<QuoteDto?> GetByIdAsync(Guid id)
        {
            var q = await GetEntityAsync(id);
            return q == null ? null : ToDto(q);
        }

        public async Task<Quote?> GetEntityAsync(Guid id)
        {
            return await _context.Quotes
                .Include(q => q.Company)
                .Include(q => q.Client)
                .Include(q => q.LineItems.OrderBy(l => l.Order))
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<QuoteDto?> CreateAsync(CreateQuoteDto dto, Guid? userId)
        {
            var company = await _context.Companies.FindAsync(dto.CompanyId);
            var client = await _context.Clients.FindAsync(dto.ClientId);
            if (company == null || client == null) return null;

            var quoteNumber = await _numbering.GenerateQuoteNumberAsync(dto.CompanyId);

            var quote = new Quote
            {
                QuoteNumber = quoteNumber,
                CompanyId = dto.CompanyId,
                ClientId = dto.ClientId,
                IssueDate = dto.IssueDate.ToUniversalTime(),
                ValidUntil = dto.ValidUntil.ToUniversalTime(),
                Currency = dto.Currency,
                VatRate = dto.VatRate,
                Subject = dto.Subject,
                Notes = dto.Notes,
                PaymentTerms = dto.PaymentTerms,
                DiscountPercent = dto.DiscountPercent,
                Status = QuoteStatus.Draft,
                CreatedByUserId = userId
            };

            // Lignes
            int order = 1;
            foreach (var line in dto.LineItems)
            {
                quote.LineItems.Add(new QuoteLineItem
                {
                    Order = order++,
                    Description = line.Description,
                    Unit = line.Unit,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountPercent = line.DiscountPercent
                });
            }

            // Calculs
            RecalculateTotals(quote);

            _context.Quotes.Add(quote);
            await _context.SaveChangesAsync();

            quote.Company = company;
            quote.Client = client;

            return ToDto(quote);
        }

        public async Task<QuoteDto?> UpdateAsync(Guid id, CreateQuoteDto dto)
        {
            var quote = await GetEntityAsync(id);
            if (quote == null) return null;

            // Protection : ne pas modifier un devis converti/accepté
            if (quote.Status == QuoteStatus.Converted || quote.Status == QuoteStatus.Accepted)
                return null;

            quote.CompanyId = dto.CompanyId;
            quote.ClientId = dto.ClientId;
            quote.IssueDate = dto.IssueDate.ToUniversalTime();
            quote.ValidUntil = dto.ValidUntil.ToUniversalTime();
            quote.Currency = dto.Currency;
            quote.VatRate = dto.VatRate;
            quote.Subject = dto.Subject;
            quote.Notes = dto.Notes;
            quote.PaymentTerms = dto.PaymentTerms;
            quote.DiscountPercent = dto.DiscountPercent;
            quote.UpdatedAt = DateTime.UtcNow;

            // Supprimer anciennes lignes + recréer
            _context.QuoteLineItems.RemoveRange(quote.LineItems);
            quote.LineItems.Clear();

            int order = 1;
            foreach (var line in dto.LineItems)
            {
                quote.LineItems.Add(new QuoteLineItem
                {
                    Order = order++,
                    Description = line.Description,
                    Unit = line.Unit,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountPercent = line.DiscountPercent
                });
            }

            RecalculateTotals(quote);
            await _context.SaveChangesAsync();

            return ToDto(quote);
        }

        public async Task<QuoteDto?> UpdateStatusAsync(Guid id, QuoteStatus status, string? rejectionReason = null)
        {
            var quote = await GetEntityAsync(id);
            if (quote == null) return null;

            quote.Status = status;
            quote.UpdatedAt = DateTime.UtcNow;

            switch (status)
            {
                case QuoteStatus.Sent:
                    quote.SentAt = DateTime.UtcNow;
                    break;
                case QuoteStatus.Accepted:
                    quote.AcceptedAt = DateTime.UtcNow;
                    break;
                case QuoteStatus.Rejected:
                    quote.RejectedAt = DateTime.UtcNow;
                    quote.RejectionReason = rejectionReason;
                    break;
            }

            await _context.SaveChangesAsync();
            return ToDto(quote);
        }

        public async Task<InvoiceDto?> ConvertToInvoiceAsync(Guid quoteId, Guid? userId)
        {
            var quote = await GetEntityAsync(quoteId);
            if (quote == null) return null;
            if (quote.Status == QuoteStatus.Converted)
                throw new InvalidOperationException("Ce devis est déjà converti");

            // Créer la facture depuis le devis
            var invoiceDto = new CreateInvoiceDto(
                quote.CompanyId,
                quote.ClientId,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(30),
                quote.Currency,
                quote.VatRate,
                quote.Subject,
                quote.Notes,
                quote.PaymentTerms,
                quote.DiscountPercent,
                quote.LineItems.OrderBy(l => l.Order).Select(l => new CreateLineItemDto(
                    l.Order, l.Description, l.Unit,
                    l.Quantity, l.UnitPrice, l.DiscountPercent
                )).ToList()
            );

            var invoice = await _invoiceService.CreateAsync(invoiceDto, userId);
            if (invoice == null) return null;

            // Mettre à jour le devis
            quote.Status = QuoteStatus.Converted;
            quote.ConvertedToInvoiceId = invoice.Id;
            quote.ConvertedAt = DateTime.UtcNow;
            quote.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Lier la facture au devis
            var invoiceEntity = await _context.Invoices.FindAsync(invoice.Id);
            if (invoiceEntity != null)
            {
                invoiceEntity.FromQuoteId = quote.Id;
                await _context.SaveChangesAsync();
            }

            return invoice;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var quote = await _context.Quotes.FindAsync(id);
            if (quote == null) return false;
            if (quote.Status == QuoteStatus.Converted)
                throw new InvalidOperationException("Impossible de supprimer un devis converti en facture");

            _context.Quotes.Remove(quote);
            await _context.SaveChangesAsync();
            return true;
        }

        // ═══════════════════════════════════════
        // Helpers
        // ═══════════════════════════════════════
        private static void RecalculateTotals(Quote quote)
        {
            var totals = BillingCalculator.Calculate(
                quote.LineItems.Select(l => (l.Quantity, l.UnitPrice, l.DiscountPercent)),
                quote.DiscountPercent,
                quote.VatRate
            );

            quote.SubtotalHT = totals.SubtotalHT;
            quote.DiscountAmount = totals.DiscountAmount;
            quote.VatAmount = totals.VatAmount;
            quote.TotalTTC = totals.TotalTTC;
        }

        private static QuoteDto ToDto(Quote q) => new(
            q.Id, q.QuoteNumber,
            q.CompanyId, q.Company?.Name ?? "", q.Company?.Color ?? "#1e3a8a", q.Company?.Logo,
            q.ClientId, q.Client?.Name ?? "", q.Client?.DisplayName ?? "",
            q.IssueDate, q.ValidUntil,
            q.Currency, q.VatRate,
            q.Subject, q.Notes, q.PaymentTerms,
            q.SubtotalHT, q.VatAmount, q.TotalTTC,
            q.DiscountPercent, q.DiscountAmount,
            q.Status.ToString(), q.IsExpired,
            q.SentAt, q.AcceptedAt, q.RejectedAt,
            q.ConvertedToInvoiceId,
            q.LineItems.OrderBy(l => l.Order).Select(l => new LineItemDto(
                l.Id, l.Order, l.Description, l.Unit,
                l.Quantity, l.UnitPrice, l.DiscountPercent, l.LineTotal
            )).ToList(),
            q.CreatedAt
        );
    }
}