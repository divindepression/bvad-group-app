namespace BvadGroupApi.Models
{
    public enum LeaveRequestStatus
    {
        Pending = 0,      // En attente
        Approved = 1,     // Approuvé
        Rejected = 2,     // Refusé
        Cancelled = 3,    // Annulé par l'employé
        Taken = 4         // Effectué (marqué manuellement ou auto par date)
    }
}