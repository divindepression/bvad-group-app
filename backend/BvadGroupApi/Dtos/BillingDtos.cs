using BvadGroupApi.Models;

namespace BvadGroupApi.Dtos
{
    // ═══════════════════════════════════════
    // CLIENT
    // ═══════════════════════════════════════
    public record ClientDto(
        Guid Id, string? ClientCode, string Type, string Name, string DisplayName,
        string? ContactPerson, string? Position,
        string? LegalForm, string? RegistrationNumber, string? TaxNumber, decimal? Capital,
        string? Email, string? Phone, string? SecondaryPhone, string? Website,
        string? Address, string? City, string? Country, string? PostalCode,
        string? Notes, bool IsActive, DateTime CreatedAt
    );

    public record CreateClientDto(
        ClientType Type, string Name, string? ContactPerson, string? Position,
        string? LegalForm, string? RegistrationNumber, string? TaxNumber, decimal? Capital,
        string? Email, string? Phone, string? SecondaryPhone, string? Website,
        string? Address, string? City, string? Country, string? PostalCode,
        string? Notes
    );

    // ═══════════════════════════════════════
    // LINE ITEM (partagé Quote + Invoice)
    // ═══════════════════════════════════════
    public record LineItemDto(
        Guid Id, int Order, string Description, string? Unit,
        decimal Quantity, decimal UnitPrice, decimal DiscountPercent, decimal LineTotal
    );

    public record CreateLineItemDto(
        int Order, string Description, string? Unit,
        decimal Quantity, decimal UnitPrice, decimal DiscountPercent
    );

    // ═══════════════════════════════════════
    // QUOTE
    // ═══════════════════════════════════════
    public record QuoteDto(
        Guid Id, string QuoteNumber,
        Guid CompanyId, string CompanyName, string CompanyColor, string? CompanyLogo,
        Guid ClientId, string ClientName, string ClientDisplayName,
        DateTime IssueDate, DateTime ValidUntil,
        string Currency, decimal VatRate,
        string? Subject, string? Notes, string? PaymentTerms,
        decimal SubtotalHT, decimal VatAmount, decimal TotalTTC,
        decimal DiscountPercent, decimal DiscountAmount,
        string Status, bool IsExpired,
        DateTime? SentAt, DateTime? AcceptedAt, DateTime? RejectedAt,
        Guid? ConvertedToInvoiceId,
        List<LineItemDto> LineItems,
        DateTime CreatedAt
    );

    public record CreateQuoteDto(
        Guid CompanyId, Guid ClientId,
        DateTime IssueDate, DateTime ValidUntil,
        string Currency, decimal VatRate,
        string? Subject, string? Notes, string? PaymentTerms,
        decimal DiscountPercent,
        List<CreateLineItemDto> LineItems
    );

    // ═══════════════════════════════════════
    // INVOICE
    // ═══════════════════════════════════════
    public record InvoiceDto(
        Guid Id, string InvoiceNumber,
        Guid CompanyId, string CompanyName, string CompanyColor, string? CompanyLogo,
        Guid ClientId, string ClientName, string ClientDisplayName,
        DateTime IssueDate, DateTime DueDate, DateTime? PaidAt,
        string Currency, decimal VatRate,
        string? Subject, string? Notes, string? PaymentTerms,
        decimal SubtotalHT, decimal VatAmount, decimal TotalTTC,
        decimal DiscountPercent, decimal DiscountAmount,
        decimal AmountPaid, decimal AmountDue,
        string Status, bool IsOverdue, int DaysOverdue,
        Guid? FromQuoteId,
        List<LineItemDto> LineItems,
        List<PaymentDto> Payments,
        DateTime CreatedAt
    );

    public record CreateInvoiceDto(
        Guid CompanyId, Guid ClientId,
        DateTime IssueDate, DateTime DueDate,
        string Currency, decimal VatRate,
        string? Subject, string? Notes, string? PaymentTerms,
        decimal DiscountPercent,
        List<CreateLineItemDto> LineItems
    );

    // ═══════════════════════════════════════
    // PAYMENT
    // ═══════════════════════════════════════
    public record PaymentDto(
        Guid Id, string? PaymentNumber, Guid InvoiceId,
        decimal Amount, string Currency,
        DateTime PaymentDate, string Method, string? MobileMoneyOperator,
        string? Reference, string? Notes,
        string? RecordedByName,
        DateTime CreatedAt
    );

    public record CreatePaymentDto(
        Guid InvoiceId, decimal Amount, string Currency,
        DateTime PaymentDate, PaymentMethod Method,
        MobileMoneyOperator? MobileMoneyOperator,
        string? Reference, string? Notes
    );

    // ═══════════════════════════════════════
    // Filtres
    // ═══════════════════════════════════════
    public record ClientFilters(string? Search, bool? IsActive);
    public record QuoteFilters(Guid? CompanyId, Guid? ClientId, QuoteStatus? Status);
    public record InvoiceFilters(Guid? CompanyId, Guid? ClientId, InvoiceStatus? Status, bool? Overdue);
}