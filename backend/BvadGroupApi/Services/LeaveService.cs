using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    public interface ILeaveService
    {
        Task<List<LeaveTypeDto>> GetTypesAsync();
        Task<List<LeaveBalanceDto>> GetBalancesAsync(Guid employeeId, int? year = null);
        Task<List<LeaveRequestDto>> GetRequestsAsync(LeaveFilters filters);
        Task<LeaveRequestDto?> GetRequestByIdAsync(Guid id);
        Task<LeaveRequestDto?> CreateRequestAsync(CreateLeaveRequestDto dto);
        Task<LeaveRequestDto?> ApproveAsync(Guid id, Guid approverUserId, string? comment);
        Task<LeaveRequestDto?> RejectAsync(Guid id, Guid approverUserId, string comment);
        Task<bool> CancelAsync(Guid id, Guid userId);
        Task<List<CalendarLeaveDto>> GetCalendarAsync(Guid companyId, DateTime fromDate, DateTime toDate);
    }

    public class LeaveService : ILeaveService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LeaveService> _logger;

        public LeaveService(AppDbContext context, ILogger<LeaveService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ═══════════════════════════════════════
        // Types de congés
        // ═══════════════════════════════════════
        public async Task<List<LeaveTypeDto>> GetTypesAsync()
        {
            var types = await _context.LeaveTypes
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();

            return types.Select(TypeToDto).ToList();
        }

        // ═══════════════════════════════════════
        // Soldes
        // ═══════════════════════════════════════
        public async Task<List<LeaveBalanceDto>> GetBalancesAsync(Guid employeeId, int? year = null)
        {
            year ??= DateTime.UtcNow.Year;

            // Recalcule le solde CP en fonction des mois travaillés + demandes approuvées
            await RefreshBalancesAsync(employeeId, year.Value);

            var balances = await _context.LeaveBalances
                .Include(b => b.LeaveType)
                .Include(b => b.Employee)
                .Where(b => b.EmployeeId == employeeId && b.Year == year)
                .OrderBy(b => b.LeaveType.DisplayOrder)
                .ToListAsync();

            return balances.Select(BalanceToDto).ToList();
        }

        private async Task RefreshBalancesAsync(Guid employeeId, int year)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return;

            var cpType = await _context.LeaveTypes.FirstOrDefaultAsync(t => t.Code == "CP");
            if (cpType == null) return;

            var balance = await _context.LeaveBalances.FirstOrDefaultAsync(
                b => b.EmployeeId == employeeId && b.LeaveTypeId == cpType.Id && b.Year == year);

            if (balance == null)
            {
                balance = new LeaveBalance
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = cpType.Id,
                    Year = year
                };
                _context.LeaveBalances.Add(balance);
            }

            // Calculer jours acquis
            var startOfYear = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now = DateTime.UtcNow;
            var start = employee.HireDate > startOfYear ? employee.HireDate : startOfYear;
            var end = now.Year == year ? now : new DateTime(year, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            var months = end < start ? 0 : (end.Year - start.Year) * 12 + (end.Month - start.Month) + 1;
            balance.AllocatedDays = Math.Min(26, months * cpType.DaysAccruedPerMonth);

            // Calculer jours consommés (demandes approuvées)
            var usedDays = await _context.LeaveRequests
                .Where(r => r.EmployeeId == employeeId
                         && r.LeaveTypeId == cpType.Id
                         && r.Status == LeaveRequestStatus.Approved
                         && r.StartDate.Year == year)
                .SumAsync(r => (decimal?)r.DaysCount) ?? 0;

            balance.UsedDays = usedDays;
            balance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ═══════════════════════════════════════
        // Demandes de congé
        // ═══════════════════════════════════════
        public async Task<List<LeaveRequestDto>> GetRequestsAsync(LeaveFilters filters)
        {
            var query = _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.Company)
                .Include(r => r.LeaveType)
                .Include(r => r.ApprovedByUser)
                .AsQueryable();

            if (filters.CompanyId.HasValue)
                query = query.Where(r => r.CompanyId == filters.CompanyId);

            if (filters.EmployeeId.HasValue)
                query = query.Where(r => r.EmployeeId == filters.EmployeeId);

            if (filters.Status.HasValue)
                query = query.Where(r => r.Status == filters.Status);

            if (filters.FromDate.HasValue)
                query = query.Where(r => r.EndDate >= filters.FromDate);

            if (filters.ToDate.HasValue)
                query = query.Where(r => r.StartDate <= filters.ToDate);

            var list = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return list.Select(RequestToDto).ToList();
        }

        public async Task<LeaveRequestDto?> GetRequestByIdAsync(Guid id)
        {
            var r = await _context.LeaveRequests
                .Include(x => x.Employee)
                .Include(x => x.Company)
                .Include(x => x.LeaveType)
                .Include(x => x.ApprovedByUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            return r == null ? null : RequestToDto(r);
        }

        public async Task<LeaveRequestDto?> CreateRequestAsync(CreateLeaveRequestDto dto)
        {
            var employee = await _context.Employees
                .Include(e => e.Company)
                .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);
            if (employee == null) return null;

            var leaveType = await _context.LeaveTypes.FindAsync(dto.LeaveTypeId);
            if (leaveType == null) return null;

            // Calcul jours ouvrés
            var days = BusinessDaysHelper.CountBusinessDays(dto.StartDate, dto.EndDate);
            if (dto.IsHalfDay && days == 1) days = 0; // pour la conversion en demi

            var request = new LeaveRequest
            {
                EmployeeId = employee.Id,
                CompanyId = employee.CompanyId,
                LeaveTypeId = leaveType.Id,
                StartDate = dto.StartDate.ToUniversalTime(),
                EndDate = dto.EndDate.ToUniversalTime(),
                IsHalfDay = dto.IsHalfDay,
                DaysCount = dto.IsHalfDay ? 0.5m : days,
                Reason = dto.Reason,
                Status = LeaveRequestStatus.Pending
            };

            _context.LeaveRequests.Add(request);
            await _context.SaveChangesAsync();

            request.Employee = employee;
            request.Company = employee.Company;
            request.LeaveType = leaveType;

            _logger.LogInformation("📝 Demande de congé créée : {Employee} - {Type} du {Start} au {End}",
                employee.FullName, leaveType.Name, dto.StartDate, dto.EndDate);

            return RequestToDto(request);
        }

        public async Task<LeaveRequestDto?> ApproveAsync(Guid id, Guid approverUserId, string? comment)
        {
            var request = await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.Company)
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return null;
            if (request.Status != LeaveRequestStatus.Pending) return null;

            request.Status = LeaveRequestStatus.Approved;
            request.ApprovedByUserId = approverUserId;
            request.ApprovedAt = DateTime.UtcNow;
            request.ApprovalComment = comment;
            request.UpdatedAt = DateTime.UtcNow;

            // Décrémenter le solde si applicable
            if (request.LeaveType.DecrementsBalance)
            {
                await RefreshBalancesAsync(request.EmployeeId, request.StartDate.Year);
            }

            await _context.SaveChangesAsync();

            var approver = await _context.Users.FindAsync(approverUserId);
            request.ApprovedByUser = approver;

            _logger.LogInformation("✅ Congé approuvé : {Employee} - {Type}",
                request.Employee?.FullName, request.LeaveType.Name);

            return RequestToDto(request);
        }

        public async Task<LeaveRequestDto?> RejectAsync(Guid id, Guid approverUserId, string comment)
        {
            var request = await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.Company)
                .Include(r => r.LeaveType)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return null;
            if (request.Status != LeaveRequestStatus.Pending) return null;

            request.Status = LeaveRequestStatus.Rejected;
            request.ApprovedByUserId = approverUserId;
            request.ApprovedAt = DateTime.UtcNow;
            request.ApprovalComment = comment;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var approver = await _context.Users.FindAsync(approverUserId);
            request.ApprovedByUser = approver;

            _logger.LogInformation("❌ Congé refusé : {Employee} - Motif: {Comment}",
                request.Employee?.FullName, comment);

            return RequestToDto(request);
        }

        public async Task<bool> CancelAsync(Guid id, Guid userId)
        {
            var request = await _context.LeaveRequests
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return false;
            if (request.Status != LeaveRequestStatus.Pending) return false;

            request.Status = LeaveRequestStatus.Cancelled;
            request.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        // ═══════════════════════════════════════
        // Calendrier équipe
        // ═══════════════════════════════════════
        public async Task<List<CalendarLeaveDto>> GetCalendarAsync(Guid companyId, DateTime fromDate, DateTime toDate)
        {
            var requests = await _context.LeaveRequests
                .Include(r => r.Employee)
                .Include(r => r.LeaveType)
                .Where(r => r.CompanyId == companyId
                         && r.Status == LeaveRequestStatus.Approved
                         && r.EndDate >= fromDate
                         && r.StartDate <= toDate)
                .OrderBy(r => r.StartDate)
                .ToListAsync();

            return requests.Select(r => new CalendarLeaveDto(
                r.Id,
                r.EmployeeId,
                r.Employee.FullName,
                r.LeaveType.Code,
                r.LeaveType.Name,
                r.LeaveType.Icon,
                r.LeaveType.Color,
                r.StartDate,
                r.EndDate,
                r.DaysCount,
                r.Status.ToString()
            )).ToList();
        }

        // ═══════════════════════════════════════
        // Mappers
        // ═══════════════════════════════════════
        private static LeaveTypeDto TypeToDto(LeaveType t) =>
            new(t.Id, t.Code, t.Name, t.Description, t.Icon, t.Color,
                t.DefaultDaysPerYear, t.DaysAccruedPerMonth, t.IsPaid,
                t.RequiresProof, t.DecrementsBalance, t.DisplayOrder, t.IsActive);

        private static LeaveBalanceDto BalanceToDto(LeaveBalance b) =>
            new(b.Id, b.EmployeeId, b.Employee?.FullName ?? "",
                b.LeaveTypeId, b.LeaveType?.Name ?? "",
                b.LeaveType?.Icon ?? "🏖", b.LeaveType?.Color ?? "#3b82f6",
                b.Year, b.AllocatedDays, b.UsedDays,
                b.CarriedOverDays, b.Adjustment, b.RemainingDays);

        private static LeaveRequestDto RequestToDto(LeaveRequest r) =>
            new(
                r.Id, r.EmployeeId, r.Employee?.FullName ?? "",
                r.Employee?.IdentityPhotoUrl,
                r.CompanyId, r.Company?.Name ?? "", r.Company?.Color ?? "#1e3a8a",
                r.LeaveTypeId, r.LeaveType?.Code ?? "", r.LeaveType?.Name ?? "",
                r.LeaveType?.Icon ?? "🏖", r.LeaveType?.Color ?? "#3b82f6",
                r.StartDate, r.EndDate, r.DaysCount, r.IsHalfDay,
                r.Reason, r.ProofDocumentUrl, r.ProofDocumentName,
                r.Status.ToString(),
                r.ApprovedByUserId, r.ApprovedByUser?.FullName,
                r.ApprovedAt, r.ApprovalComment,
                r.IsPast, r.IsCurrent, r.IsFuture,
                r.CreatedAt
            );
    }
}