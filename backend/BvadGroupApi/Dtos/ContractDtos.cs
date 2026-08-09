using BvadGroupApi.Models;

namespace BvadGroupApi.Dtos
{
    public record ContractDto(
        Guid Id,
        Guid EmployeeId,
        string EmployeeName,
        string? EmployeePosition,
        Guid CompanyId,
        string CompanyName,
        string CompanyColor,
        string? CompanyLogo,
        string ContractNumber,
        string ContractType,
        string Status,
        string Position,
        string? Department,
        DateTime StartDate,
        DateTime? EndDate,
        DateTime? SignedDate,
        decimal Salary,
        string Currency,
        int? TrialPeriodMonths,
        int? WeeklyHours,
        string? DocumentUrl,
        string? DocumentFileName,
        long? DocumentSize,
        string? SpecialClauses,
        string? Notes,
        int? RemainingDays,
        bool IsExpiringSoon,
        bool IsExpired,
        DateTime CreatedAt
    );

    public record CreateContractDto(
        Guid EmployeeId,
        ContractType ContractType,
        string Position,
        string? Department,
        DateTime StartDate,
        DateTime? EndDate,
        DateTime? SignedDate,
        decimal Salary,
        string Currency,
        int? TrialPeriodMonths,
        int? WeeklyHours,
        string? SpecialClauses,
        string? Notes,
        ContractStatus Status = ContractStatus.Draft
    );

    public record UpdateContractDto(
        ContractType ContractType,
        string Position,
        string? Department,
        DateTime StartDate,
        DateTime? EndDate,
        DateTime? SignedDate,
        decimal Salary,
        string Currency,
        int? TrialPeriodMonths,
        int? WeeklyHours,
        string? SpecialClauses,
        string? Notes,
        ContractStatus Status
    );

    public record ContractFilters(
        Guid? CompanyId,
        Guid? EmployeeId,
        ContractStatus? Status,
        ContractType? Type,
        bool? ExpiringSoon
    );
}