using BvadGroupApi.Models;

namespace BvadGroupApi.Dtos
{
    // ============ Réponse (API → Front) ============
    public record EmployeeDto(
        Guid Id,
        string FirstName,
        string LastName,
        string? MiddleName,
        string FullName,
        string Email,
        string? PhoneNumber,
        string Position,
        string? Department,
        string Status,
        string ContractType,
        DateTime HireDate,
        DateTime? EndDate,
        decimal? Salary,
        DateTime? BirthDate,
        int? Age,
        string Gender,
        string? City,
        string? Country,
        string? PhotoUrl,
        Guid CompanyId,
        string CompanyName,
        string CompanyColor,
        string? CompanyLogo,
        DateTime CreatedAt
    );

    // ============ Requêtes (Front → API) ============
    public record CreateEmployeeDto(
        string FirstName,
        string LastName,
        string? MiddleName,
        string Email,
        string? PhoneNumber,
        string Position,
        string? Department,
        Gender Gender,
        DateTime? BirthDate,
        DateTime HireDate,
        DateTime? EndDate,
        ContractType ContractType,
        decimal? Salary,
        EmployeeStatus Status,
        string? City,
        string? Country,
        Guid CompanyId,
        string? PhotoUrl,
        string? Notes
    );

    public record UpdateEmployeeDto(
        string FirstName,
        string LastName,
        string? MiddleName,
        string Email,
        string? PhoneNumber,
        string Position,
        string? Department,
        Gender Gender,
        DateTime? BirthDate,
        DateTime HireDate,
        DateTime? EndDate,
        ContractType ContractType,
        decimal? Salary,
        EmployeeStatus Status,
        string? City,
        string? Country,
        Guid CompanyId,
        string? PhotoUrl,
        string? Notes
    );

    // ============ Filtres ============
    public record EmployeeFilters(
        Guid? CompanyId,
        string? Search,
        EmployeeStatus? Status,
        string? Department
    );
}