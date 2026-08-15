using BvadGroupApi.Models;

namespace BvadGroupApi.Dtos
{
    // ============ LeaveType ============
    public record LeaveTypeDto(
        Guid Id,
        string Code,
        string Name,
        string? Description,
        string Icon,
        string Color,
        int DefaultDaysPerYear,
        decimal DaysAccruedPerMonth,
        bool IsPaid,
        bool RequiresProof,
        bool DecrementsBalance,
        int DisplayOrder,
        bool IsActive
    );

    // ============ LeaveBalance ============
    public record LeaveBalanceDto(
        Guid Id,
        Guid EmployeeId,
        string EmployeeName,
        Guid LeaveTypeId,
        string LeaveTypeName,
        string LeaveTypeIcon,
        string LeaveTypeColor,
        int Year,
        decimal AllocatedDays,
        decimal UsedDays,
        decimal CarriedOverDays,
        decimal Adjustment,
        decimal RemainingDays
    );

    // ============ LeaveRequest ============
    public record LeaveRequestDto(
        Guid Id,
        Guid EmployeeId,
        string EmployeeName,
        string? EmployeePhotoUrl,
        Guid CompanyId,
        string CompanyName,
        string CompanyColor,
        Guid LeaveTypeId,
        string LeaveTypeCode,
        string LeaveTypeName,
        string LeaveTypeIcon,
        string LeaveTypeColor,
        DateTime StartDate,
        DateTime EndDate,
        decimal DaysCount,
        bool IsHalfDay,
        string? Reason,
        string? ProofDocumentUrl,
        string? ProofDocumentName,
        string Status,
        Guid? ApprovedByUserId,
        string? ApprovedByName,
        DateTime? ApprovedAt,
        string? ApprovalComment,
        bool IsPast,
        bool IsCurrent,
        bool IsFuture,
        DateTime CreatedAt
    );

    // ============ Requêtes ============
    public record CreateLeaveRequestDto(
        Guid EmployeeId,
        Guid LeaveTypeId,
        DateTime StartDate,
        DateTime EndDate,
        bool IsHalfDay,
        string? Reason
    );

    public record ApproveLeaveDto(
        string? Comment
    );

    public record RejectLeaveDto(
        string Comment  // Obligatoire pour refus
    );

    public record LeaveFilters(
        Guid? CompanyId,
        Guid? EmployeeId,
        LeaveRequestStatus? Status,
        DateTime? FromDate,
        DateTime? ToDate
    );

    // ============ Calendrier ============
    public record CalendarLeaveDto(
        Guid Id,
        Guid EmployeeId,
        string EmployeeName,
        string LeaveTypeCode,
        string LeaveTypeName,
        string LeaveTypeIcon,
        string LeaveTypeColor,
        DateTime StartDate,
        DateTime EndDate,
        decimal DaysCount,
        string Status
    );
}