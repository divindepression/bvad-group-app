namespace BvadGroupApi.Models
{
    public enum NotificationType
    {
        // 🏖 Congés
        LeaveRequestSubmitted = 1,      // Nouvelle demande à approuver
        LeaveRequestApproved = 2,       // Ta demande a été approuvée
        LeaveRequestRejected = 3,       // Ta demande a été refusée
        LeaveRequestCancelled = 4,      // Une demande a été annulée

        // 📄 Contrats
        ContractExpiringSoon = 10,      // Contrat expire dans 30j
        ContractExpired = 11,           // Contrat a expiré
        ContractCreated = 12,           // Nouveau contrat créé
        ContractRenewed = 13,           // Contrat renouvelé

        // 👤 Employés
        EmployeeHired = 20,             // Nouvel employé embauché
        EmployeeLeft = 21,              // Employé parti
        EmployeeBirthday = 22,          // Anniversaire

        // 🏢 Système
        SystemAlert = 90,               // Alerte système
        Info = 99                       // Info générique
    }

    public enum NotificationPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Urgent = 3
    }
}