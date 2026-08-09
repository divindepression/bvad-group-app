namespace BvadGroupApi.Models
{
    /// <summary>Statut d'un employé</summary>
    public enum EmployeeStatus
    {
        Active = 0,      // Actif
        OnLeave = 1,     // En congé
        Suspended = 2,   // Suspendu
        Terminated = 3,  // Parti / Licencié
        Probation = 4    // Période d'essai
    }

    /// <summary>Genre</summary>
    public enum Gender
    {
        Male = 0,
        Female = 1,
        Other = 2
    }

    /// <summary>Type de contrat</summary>
    public enum ContractType
    {
        CDI = 0,           // Contrat à Durée Indéterminée
        CDD = 1,           // Contrat à Durée Déterminée
        Internship = 2,    // Stage
        Freelance = 3,     // Prestation
        Apprenticeship = 4 // Apprentissage
    }
}