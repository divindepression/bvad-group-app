namespace BvadGroupApi.Dtos
{
    // ================================================
    // 📥 Requêtes (Frontend → API)
    // ================================================

    /// <summary>Requête de login</summary>
    public record LoginRequest(string Username, string Password);

    // ================================================
    // 📤 Réponses (API → Frontend)
    // ================================================

    /// <summary>Réponse après connexion réussie</summary>
    public record LoginResponse(
        string Token,
        DateTime ExpiresAt,
        UserDto User,
        List<CompanyAccessDto> Companies
    );

    /// <summary>Informations utilisateur retournées après login</summary>
    public record UserDto(
        Guid Id,
        string Username,
        string Email,
        string FirstName,
        string LastName,
        string FullName,
        string Role,
        string? PhotoUrl
    );

    /// <summary>Filiale accessible par l'utilisateur</summary>
    public record CompanyAccessDto(
        Guid Id,
        string Code,
        string Name,
        string Color,
        string? Logo,
        bool IsHolding,
        bool IsDefault,
        string? Role
    );
}