namespace BvadGroupApi.Models
{
    /// <summary>
    /// Rôles disponibles dans BVAD GROUP
    /// </summary>
    public enum UserRole
    {
        /// <summary>Super admin (Divin) - accès total à tout</summary>
        SuperAdmin = 0,

        /// <summary>Directeur de filiale - gère sa filiale</summary>
        Director = 1,

        /// <summary>Ressources Humaines - gère les employés</summary>
        HR = 2,

        /// <summary>Manager - encadre une équipe</summary>
        Manager = 3,

        /// <summary>Employé simple - accès limité</summary>
        Employee = 4
    }
}