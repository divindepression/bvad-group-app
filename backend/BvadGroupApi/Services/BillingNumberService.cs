using BvadGroupApi.Data;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    public interface IBillingNumberService
    {
        Task<string> GenerateClientCodeAsync();
        Task<string> GenerateQuoteNumberAsync(Guid companyId);
        Task<string> GenerateInvoiceNumberAsync(Guid companyId);
        Task<string> GeneratePaymentNumberAsync();
    }

    public class BillingNumberService : IBillingNumberService
    {
        private readonly AppDbContext _context;

        public BillingNumberService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>CLI-2026-0001</summary>
        public async Task<string> GenerateClientCodeAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"CLI-{year}-";

            var count = await _context.Clients
                .CountAsync(c => c.ClientCode != null && c.ClientCode.StartsWith(prefix));

            return $"{prefix}{(count + 1):D4}";
        }

        /// <summary>DEV-BVAD_TECH-2026-0001</summary>
        public async Task<string> GenerateQuoteNumberAsync(Guid companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null) throw new InvalidOperationException("Filiale introuvable");

            var year = DateTime.UtcNow.Year;
            var prefix = $"DEV-{company.Code}-{year}-";

            var count = await _context.Quotes
                .CountAsync(q => q.CompanyId == companyId && q.QuoteNumber.StartsWith(prefix));

            return $"{prefix}{(count + 1):D4}";
        }

        /// <summary>FAC-BVAD_TECH-2026-0001</summary>
        public async Task<string> GenerateInvoiceNumberAsync(Guid companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null) throw new InvalidOperationException("Filiale introuvable");

            var year = DateTime.UtcNow.Year;
            var prefix = $"FAC-{company.Code}-{year}-";

            var count = await _context.Invoices
                .CountAsync(i => i.CompanyId == companyId && i.InvoiceNumber.StartsWith(prefix));

            return $"{prefix}{(count + 1):D4}";
        }

        /// <summary>PAY-2026-0001</summary>
        public async Task<string> GeneratePaymentNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"PAY-{year}-";

            var count = await _context.Payments
                .CountAsync(p => p.PaymentNumber.StartsWith(prefix));

            return $"{prefix}{(count + 1):D4}";
        }
    }
}