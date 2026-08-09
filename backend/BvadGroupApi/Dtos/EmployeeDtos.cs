using BvadGroupApi.Models;

namespace BvadGroupApi.Dtos
{
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
        string CompanyRole,
        bool IsCommitteeMember,
        string CommitteePosition,
        string? CommitteePositionCustom,
        Guid? ManagerId,
        string? ManagerName,
        Guid? UserId,
        DateTime CreatedAt,

    // 🆕 NOUVEAUX CHAMPS
    string? EmployeeNumber,
    string? IdentityPhotoUrl,
    string? SignatureUrl,
    string? BankName,
    string? BankAccountNumber,
    string? MobileMoneyOperator,
    string? MobileMoneyNumber,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? EmergencyContactRelation,
    string? NationalIdNumber,
    string? SocialSecurityNumber

    );

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
        string? Notes,
        UserRole CompanyRole = UserRole.Employee,
        bool IsCommitteeMember = false,
        CommitteePosition CommitteePosition = CommitteePosition.None,
        string? CommitteePositionCustom = null,
        Guid? ManagerId = null
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
        string? Notes,
        UserRole CompanyRole = UserRole.Employee,
        bool IsCommitteeMember = false,
        CommitteePosition CommitteePosition = CommitteePosition.None,
        string? CommitteePositionCustom = null,
        Guid? ManagerId = null
    );

    public record EmployeeFilters(
        Guid? CompanyId,
        string? Search,
        EmployeeStatus? Status,
        string? Department
    );
}