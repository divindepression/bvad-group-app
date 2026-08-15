namespace BvadGroupApi.Models
{
    /// <summary>
    /// Facture émise à un client.
    /// </summary>
    public class Invoice
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // 🆔 Numéro auto (FAC-BVAD_TECH-2026-0001)
        public string InvoiceNumber { get; set; } = string.Empty;

        // 🏢 Filiale
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // 👤 Client
        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;

        // 📅 Dates
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);
        public DateTime? PaidAt { get; set; }

        // 💰 Devise + TVA
        public string Currency { get; set; } = "XAF";
        public decimal VatRate { get; set; } = 18m;

        // 📝 Contenu
        public string? Subject { get; set; }
        public string? Notes { get; set; }
        public string? PaymentTerms { get; set; }

        // 🧮 Totaux
        public decimal SubtotalHT { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalTTC { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }

        // 💳 Paiements
        public decimal AmountPaid { get; set; }
        public decimal AmountDue => TotalTTC - AmountPaid;

        // 🔄 Statut
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        // 🔗 Origine (devis)
        public Guid? FromQuoteId { get; set; }
        public Quote? FromQuote { get; set; }

        // 👤 Créateur
        public Guid? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        // 🕒 Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 📋 Lignes + Paiements
        public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // 🧮 Calculé
        public bool IsOverdue => Status != InvoiceStatus.Paid
                              && Status != InvoiceStatus.Cancelled
                              && DueDate < DateTime.UtcNow
                              && AmountDue > 0;

        public int DaysOverdue => IsOverdue ? (int)(DateTime.UtcNow - DueDate).TotalDays : 0;
    }

    public class InvoiceLineItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public int Order { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal LineTotal => Quantity * UnitPrice * (1 - DiscountPercent / 100m);
    }

    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public string PaymentNumber { get; set; } = string.Empty;  // ex: PAY-2026-0001

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "XAF";
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public PaymentMethod Method { get; set; }
        public MobileMoneyOperator? MobileMoneyOperator { get; set; }

        public string? Reference { get; set; }             // N° transaction / chèque
        public string? Notes { get; set; }

        // 👤 Enregistré par
        public Guid? RecordedByUserId { get; set; }
        public User? RecordedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}