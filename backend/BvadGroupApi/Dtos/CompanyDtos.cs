namespace BvadGroupApi.Dtos
{
    public record CompanyDto(
        Guid Id,
        string Code,
        string Name,
        string? LegalName,
        string? Description,
        string? Slogan,
        string Color,
        string? Logo,
        string? LogoUrl,
        string? StampUrl,
        string? DirectorSignatureUrl,
        string? RegistrationNumber,
        string? TaxNumber,
        string? Address,
        string? City,
        string? Country,
        string? Phone,
        string? Email,
        string? Website,
        string? DirectorName,
        string? DirectorTitle,
        bool IsHolding,
        int DisplayOrder,
        bool IsActive
    );

    public record UpdateCompanyDto(
        string Name,
        string? LegalName,
        string? Description,
        string? Slogan,
        string Color,
        string? RegistrationNumber,
        string? TaxNumber,
        string? Address,
        string? City,
        string? Country,
        string? Phone,
        string? Email,
        string? Website,
        string? DirectorName,
        string? DirectorTitle
    );

    public record CreateCompanyDto(
    string Code,
    string Name,
    string? LegalName,
    string? Description,
    string? Slogan,
    string Color,
    string? Logo,                // emoji fallback
    bool IsHolding,
    int DisplayOrder,

    // Contact
    string? Address,
    string? City,
    string? Country,
    string? Phone,
    string? Email,
    string? Website,

    // Légal
    string? RegistrationNumber,
    string? TaxNumber,

    // Direction
    string? DirectorName,
    string? DirectorTitle
);
}