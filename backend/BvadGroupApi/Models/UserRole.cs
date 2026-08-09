namespace BvadGroupApi.Models
{
    /// <summary>
    /// Rôles disponibles dans le système BVAD GROUP.
    /// Un utilisateur a un rôle GLOBAL (Système) et peut avoir des rôles spécifiques par filiale.
    /// </summary>
    public enum UserRole
    {
        // ═══ NIVEAU SYSTÈME ═══
        SuperAdmin = 0,        // Divin - accès total absolu
        Admin = 1,             // Admin technique
        User = 2,              // Utilisateur standard (par défaut)

        // ═══ NIVEAU FILIALE ═══
        Director = 10,         // Directeur Général de la filiale
        Manager = 11,          // Chef de département/équipe
        HR = 12,               // Ressources Humaines
        Accountant = 13,       // Comptable
        Employee = 14          // Employé standard
    }
}