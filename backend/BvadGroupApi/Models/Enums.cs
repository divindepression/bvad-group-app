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

    /// <summary>Postes standards du comité de direction</summary>
    public enum CommitteePosition
    {
        None = 0,               // Pas membre du comité
        CEO = 1,                // Président-Directeur Général (PDG)
        DGA = 2,                // Directeur Général Adjoint
        CFO = 3,                // Directeur Financier (DAF)
        CHRO = 4,               // Directeur RH (DRH)
        CTO = 5,                // Directeur Technique (DT / CTO)
        COO = 6,                // Directeur des Opérations
        CMO = 7,                // Directeur Marketing/Commercial
        CIO = 8,                // Directeur des Systèmes d'Information
        Legal = 9,              // Directeur Juridique
        Custom = 99             // Poste personnalisé (voir CommitteePositionCustom)
    }

}