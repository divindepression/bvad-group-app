namespace BvadGroupApi.Models
{
    /// <summary>
    /// Devis (proposition commerciale).
    /// </summary>
    public class Quote
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // 🆔 Numéro auto (DEV-BVAD_TECH-2026-0001)
        public string QuoteNumber { get; set; } = string.Empty;

        // 🏢 Filiale émettrice
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // 👤 Client
        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;

        // 📅 Dates
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddDays(30);

        // 💰 Devise + TVA
        public string Currency { get; set; } = "XAF";
        public decimal VatRate { get; set; } = 18m;

        // 📝 Contenu
        public string? Subject { get; set; }             // Objet du devis
        public string? Notes { get; set; }               // Notes / conditions
        public string? PaymentTerms { get; set; }        // Conditions de paiement

        // 🧮 Totaux calculés
        public decimal SubtotalHT { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalTTC { get; set; }
        public decimal DiscountPercent { get; set; }     // Remise globale %
        public decimal DiscountAmount { get; set; }      // Remise globale montant

        // 🔄 Statut
        public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
        public DateTime? SentAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        // 🔗 Si converti en facture
        public Guid? ConvertedToInvoiceId { get; set; }
        public DateTime? ConvertedAt { get; set; }

        // 👤 Créateur
        public Guid? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        // 🕒 Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 📋 Lignes
        public ICollection<QuoteLineItem> LineItems { get; set; } = new List<QuoteLineItem>();

        // 🧮 Calculé
        public bool IsExpired => Status == QuoteStatus.Sent && ValidUntil < DateTime.UtcNow;
    }

    public class QuoteLineItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid QuoteId { get; set; }
        public Quote Quote { get; set; } = null!;

        public int Order { get; set; }                   // Ordre d'affichage
        public string Description { get; set; } = string.Empty;
        public string? Unit { get; set; }                // heure, jour, pièce, kg...
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }     // Remise par ligne
        public decimal LineTotal => Quantity * UnitPrice * (1 - DiscountPercent / 100m);
    }
}