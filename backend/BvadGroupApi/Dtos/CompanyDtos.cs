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
}