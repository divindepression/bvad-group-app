namespace BvadGroupApi.Models
{
    public enum ClientType
    {
        Individual = 0,  // Personne physique
        Company = 1      // Entreprise
    }

    public enum QuoteStatus
    {
        Draft = 0,       // Brouillon
        Sent = 1,        // Envoyé au client
        Accepted = 2,    // Accepté
        Rejected = 3,    // Refusé
        Expired = 4,     // Expiré (date passée)
        Converted = 5    // Converti en facture
    }

    public enum InvoiceStatus
    {
        Draft = 0,             // Brouillon
        Issued = 1,            // Émise
        PartiallyPaid = 2,     // Payée partiellement
        Paid = 3,              // Entièrement payée
        Overdue = 4,           // En retard
        Cancelled = 5          // Annulée
    }

    public enum PaymentMethod
    {
        Cash = 0,              // Espèces
        BankTransfer = 1,      // Virement
        MobileMoney = 2,       // Mobile Money
        Check = 3,             // Chèque
        Card = 4,              // Carte bancaire
        Other = 5              // Autre
    }

    public enum MobileMoneyOperator
    {
        None = 0,
        MTN = 1,
        Airtel = 2,
        Other = 99
    }
}