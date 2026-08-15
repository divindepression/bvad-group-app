namespace BvadGroupApi.Dtos
{
    // ═══ Vue d'ensemble ═══
    public record DashboardOverviewDto(
        int TotalEmployees,
        int ActiveEmployees,
        int TotalCompanies,
        int TotalContracts,
        int ActiveContracts,
        int ExpiringContracts,       // dans les 30j
        int PendingLeaveRequests,
        int EmployeesOnLeaveToday,
        decimal TotalMonthlySalary,
        int UpcomingBirthdays,       // 30j à venir
        int NewEmployeesThisMonth
    );

    // ═══ Employés par filiale ═══
    public record EmployeesByCompanyDto(
        Guid CompanyId,
        string CompanyName,
        string CompanyColor,
        string? CompanyLogo,
        int Count,
        int ActiveCount,
        int OnLeaveCount,
        decimal TotalSalary
    );

    // ═══ Employés par département ═══
    public record EmployeesByDepartmentDto(
        string Department,
        int Count
    );

    // ═══ Employés par type de contrat ═══
    public record EmployeesByContractDto(
        string ContractType,
        int Count
    );

    // ═══ Évolution embauches (12 derniers mois) ═══
    public record HiringTrendDto(
        int Year,
        int Month,
        string MonthLabel,
        int Count
    );

    // ═══ Congés par mois ═══
    public record LeavesByMonthDto(
        int Year,
        int Month,
        string MonthLabel,
        int TotalRequests,
        decimal TotalDays,
        int Approved,
        int Pending,
        int Rejected
    );

    // ═══ Alertes contrats ═══
    public record ExpiringContractDto(
        Guid Id,
        string ContractNumber,
        Guid EmployeeId,
        string EmployeeName,
        string CompanyName,
        string CompanyColor,
        string Position,
        DateTime EndDate,
        int DaysRemaining
    );

    // ═══ Anniversaires ═══
    public record BirthdayDto(
        Guid EmployeeId,
        string EmployeeName,
        string? PhotoUrl,
        string CompanyName,
        string CompanyColor,
        string Position,
        DateTime BirthDate,
        int AgeThisYear,
        int DaysUntil
    );

    // ═══ Absents aujourd'hui ═══
    public record AbsentTodayDto(
        Guid EmployeeId,
        string EmployeeName,
        string? PhotoUrl,
        string CompanyName,
        string CompanyColor,
        string LeaveTypeName,
        string LeaveTypeIcon,
        DateTime StartDate,
        DateTime EndDate,
        int DaysLeft
    );

    // ═══ Filtres ═══
    public record DashboardFilters(
        Guid? CompanyId
    );
}