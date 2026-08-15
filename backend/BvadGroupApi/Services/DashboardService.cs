using System.Globalization;
using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    public interface IDashboardService
    {
        Task<DashboardOverviewDto> GetOverviewAsync(Guid? companyId);
        Task<List<EmployeesByCompanyDto>> GetEmployeesByCompanyAsync();
        Task<List<EmployeesByDepartmentDto>> GetEmployeesByDepartmentAsync(Guid? companyId);
        Task<List<EmployeesByContractDto>> GetEmployeesByContractAsync(Guid? companyId);
        Task<List<HiringTrendDto>> GetHiringTrendAsync(Guid? companyId, int months = 12);
        Task<List<LeavesByMonthDto>> GetLeavesByMonthAsync(Guid? companyId, int months = 12);
        Task<List<ExpiringContractDto>> GetExpiringContractsAsync(Guid? companyId, int days = 60);
        Task<List<BirthdayDto>> GetUpcomingBirthdaysAsync(Guid? companyId, int days = 30);
        Task<List<AbsentTodayDto>> GetAbsentTodayAsync(Guid? companyId);
    }

    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        // ═══════════════════════════════════════
        // 📊 OVERVIEW
        // ═══════════════════════════════════════
        public async Task<DashboardOverviewDto> GetOverviewAsync(Guid? companyId)
        {
            var empQuery = _context.Employees.AsQueryable();
            if (companyId.HasValue) empQuery = empQuery.Where(e => e.CompanyId == companyId);

            var contractQuery = _context.Contracts.AsQueryable();
            if (companyId.HasValue) contractQuery = contractQuery.Where(c => c.CompanyId == companyId);

            var leaveQuery = _context.LeaveRequests.AsQueryable();
            if (companyId.HasValue) leaveQuery = leaveQuery.Where(l => l.CompanyId == companyId);

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var in30Days = now.AddDays(30);

            var totalEmployees = await empQuery.CountAsync();
            var activeEmployees = await empQuery.CountAsync(e => e.Status == EmployeeStatus.Active);
            var totalCompanies = await _context.Companies.CountAsync();
            var totalContracts = await contractQuery.CountAsync();
            var activeContracts = await contractQuery.CountAsync(c => c.Status == ContractStatus.Active);

            var expiringContracts = await contractQuery.CountAsync(c =>
                c.EndDate.HasValue &&
                c.EndDate.Value >= now &&
                c.EndDate.Value <= in30Days &&
                c.Status == ContractStatus.Active);

            var pendingLeaves = await leaveQuery.CountAsync(l => l.Status == LeaveRequestStatus.Pending);

            var employeesOnLeaveToday = await leaveQuery.CountAsync(l =>
                l.Status == LeaveRequestStatus.Approved &&
                l.StartDate <= now &&
                l.EndDate >= now);

            var totalSalary = await empQuery
                .Where(e => e.Status == EmployeeStatus.Active && e.Salary.HasValue)
                .SumAsync(e => e.Salary ?? 0);

            var newEmployeesThisMonth = await empQuery.CountAsync(e => e.HireDate >= startOfMonth);

            // Anniversaires 30j à venir (calcul en mémoire)
            var allEmployees = await empQuery
                .Where(e => e.BirthDate.HasValue && e.Status == EmployeeStatus.Active)
                .Select(e => new { e.Id, e.BirthDate })
                .ToListAsync();

            var upcomingBirthdays = allEmployees.Count(e =>
            {
                if (!e.BirthDate.HasValue) return false;
                var nextBirthday = GetNextBirthday(e.BirthDate.Value);
                return (nextBirthday - now).TotalDays <= 30;
            });

            return new DashboardOverviewDto(
                totalEmployees,
                activeEmployees,
                totalCompanies,
                totalContracts,
                activeContracts,
                expiringContracts,
                pendingLeaves,
                employeesOnLeaveToday,
                totalSalary,
                upcomingBirthdays,
                newEmployeesThisMonth
            );
        }

        // ═══════════════════════════════════════
        // 🏢 Employés par filiale
        // ═══════════════════════════════════════
        public async Task<List<EmployeesByCompanyDto>> GetEmployeesByCompanyAsync()
        {
            var now = DateTime.UtcNow;

            var companies = await _context.Companies
                .Where(c => !c.IsHolding)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var result = new List<EmployeesByCompanyDto>();

            foreach (var c in companies)
            {
                var emps = await _context.Employees.Where(e => e.CompanyId == c.Id).ToListAsync();

                var onLeave = await _context.LeaveRequests.CountAsync(l =>
                    l.CompanyId == c.Id &&
                    l.Status == LeaveRequestStatus.Approved &&
                    l.StartDate <= now &&
                    l.EndDate >= now);

                result.Add(new EmployeesByCompanyDto(
                    c.Id,
                    c.Name,
                    c.Color,
                    c.Logo,
                    emps.Count,
                    emps.Count(e => e.Status == EmployeeStatus.Active),
                    onLeave,
                    emps.Where(e => e.Status == EmployeeStatus.Active).Sum(e => e.Salary ?? 0)
                ));
            }

            return result;
        }

        // ═══════════════════════════════════════
        // 🏛 Employés par département
        // ═══════════════════════════════════════
        public async Task<List<EmployeesByDepartmentDto>> GetEmployeesByDepartmentAsync(Guid? companyId)
        {
            var query = _context.Employees.AsQueryable();
            if (companyId.HasValue) query = query.Where(e => e.CompanyId == companyId);

            var groups = await query
                .Where(e => !string.IsNullOrEmpty(e.Department))
                .GroupBy(e => e.Department)
                .Select(g => new EmployeesByDepartmentDto(g.Key!, g.Count()))
                .OrderByDescending(g => g.Count)
                .ToListAsync();

            return groups;
        }

        // ═══════════════════════════════════════
        // 📄 Employés par type de contrat
        // ═══════════════════════════════════════
        public async Task<List<EmployeesByContractDto>> GetEmployeesByContractAsync(Guid? companyId)
        {
            var query = _context.Employees.AsQueryable();
            if (companyId.HasValue) query = query.Where(e => e.CompanyId == companyId);

            var groups = await query
                .GroupBy(e => e.ContractType)
                .Select(g => new EmployeesByContractDto(g.Key.ToString(), g.Count()))
                .OrderByDescending(g => g.Count)
                .ToListAsync();

            return groups;
        }

        // ═══════════════════════════════════════
        // 📈 Évolution embauches
        // ═══════════════════════════════════════
        public async Task<List<HiringTrendDto>> GetHiringTrendAsync(Guid? companyId, int months = 12)
        {
            var query = _context.Employees.AsQueryable();
            if (companyId.HasValue) query = query.Where(e => e.CompanyId == companyId);

            var startDate = DateTime.UtcNow.AddMonths(-months);
            var hires = await query
                .Where(e => e.HireDate >= startDate)
                .Select(e => new { e.HireDate.Year, e.HireDate.Month })
                .ToListAsync();

            var result = new List<HiringTrendDto>();
            var culture = new CultureInfo("fr-FR");

            for (int i = months - 1; i >= 0; i--)
            {
                var d = DateTime.UtcNow.AddMonths(-i);
                var count = hires.Count(h => h.Year == d.Year && h.Month == d.Month);
                var label = culture.DateTimeFormat.GetAbbreviatedMonthName(d.Month) + " " + d.ToString("yy");
                result.Add(new HiringTrendDto(d.Year, d.Month, label, count));
            }

            return result;
        }

        // ═══════════════════════════════════════
        // 🏖 Congés par mois
        // ═══════════════════════════════════════
        public async Task<List<LeavesByMonthDto>> GetLeavesByMonthAsync(Guid? companyId, int months = 12)
        {
            var query = _context.LeaveRequests.AsQueryable();
            if (companyId.HasValue) query = query.Where(l => l.CompanyId == companyId);

            var startDate = DateTime.UtcNow.AddMonths(-months);
            var leaves = await query
                .Where(l => l.StartDate >= startDate)
                .Select(l => new { l.StartDate.Year, l.StartDate.Month, l.DaysCount, l.Status })
                .ToListAsync();

            var result = new List<LeavesByMonthDto>();
            var culture = new CultureInfo("fr-FR");

            for (int i = months - 1; i >= 0; i--)
            {
                var d = DateTime.UtcNow.AddMonths(-i);
                var monthLeaves = leaves.Where(l => l.Year == d.Year && l.Month == d.Month).ToList();
                var label = culture.DateTimeFormat.GetAbbreviatedMonthName(d.Month) + " " + d.ToString("yy");

                result.Add(new LeavesByMonthDto(
                    d.Year, d.Month, label,
                    monthLeaves.Count,
                    monthLeaves.Sum(l => l.DaysCount),
                    monthLeaves.Count(l => l.Status == LeaveRequestStatus.Approved),
                    monthLeaves.Count(l => l.Status == LeaveRequestStatus.Pending),
                    monthLeaves.Count(l => l.Status == LeaveRequestStatus.Rejected)
                ));
            }

            return result;
        }

        // ═══════════════════════════════════════
        // ⚠ Contrats expirant
        // ═══════════════════════════════════════
        public async Task<List<ExpiringContractDto>> GetExpiringContractsAsync(Guid? companyId, int days = 60)
        {
            var now = DateTime.UtcNow;
            var limit = now.AddDays(days);

            var query = _context.Contracts
                .Include(c => c.Employee)
                .Include(c => c.Company)
                .Where(c => c.EndDate.HasValue &&
                            c.EndDate.Value >= now &&
                            c.EndDate.Value <= limit &&
                            c.Status == ContractStatus.Active);

            if (companyId.HasValue) query = query.Where(c => c.CompanyId == companyId);

            var contracts = await query
                .OrderBy(c => c.EndDate)
                .ToListAsync();

            return contracts.Select(c => new ExpiringContractDto(
                c.Id,
                c.ContractNumber,
                c.EmployeeId,
                c.Employee?.FullName ?? "",
                c.Company?.Name ?? "",
                c.Company?.Color ?? "#1e3a8a",
                c.Position,
                c.EndDate!.Value,
                (int)(c.EndDate.Value - now).TotalDays
            )).ToList();
        }

        // ═══════════════════════════════════════
        // 🎂 Anniversaires
        // ═══════════════════════════════════════
        public async Task<List<BirthdayDto>> GetUpcomingBirthdaysAsync(Guid? companyId, int days = 30)
        {
            var query = _context.Employees
                .Include(e => e.Company)
                .Where(e => e.BirthDate.HasValue && e.Status == EmployeeStatus.Active);

            if (companyId.HasValue) query = query.Where(e => e.CompanyId == companyId);

            var employees = await query.ToListAsync();
            var now = DateTime.UtcNow;

            var result = employees
                .Select(e =>
                {
                    var next = GetNextBirthday(e.BirthDate!.Value);
                    var daysUntil = (int)(next - now).TotalDays;
                    var age = next.Year - e.BirthDate.Value.Year;
                    return new BirthdayDto(
                        e.Id,
                        e.FullName,
                        e.IdentityPhotoUrl ?? e.PhotoUrl,
                        e.Company?.Name ?? "",
                        e.Company?.Color ?? "#1e3a8a",
                        e.Position,
                        e.BirthDate.Value,
                        age,
                        daysUntil
                    );
                })
                .Where(b => b.DaysUntil <= days)
                .OrderBy(b => b.DaysUntil)
                .ToList();

            return result;
        }

        // ═══════════════════════════════════════
        // 🏖 Absents aujourd'hui
        // ═══════════════════════════════════════
        public async Task<List<AbsentTodayDto>> GetAbsentTodayAsync(Guid? companyId)
        {
            var now = DateTime.UtcNow;

            var query = _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.Company)
                .Include(l => l.LeaveType)
                .Where(l => l.Status == LeaveRequestStatus.Approved &&
                            l.StartDate <= now &&
                            l.EndDate >= now);

            if (companyId.HasValue) query = query.Where(l => l.CompanyId == companyId);

            var leaves = await query.ToListAsync();

            return leaves.Select(l => new AbsentTodayDto(
                l.EmployeeId,
                l.Employee?.FullName ?? "",
                l.Employee?.IdentityPhotoUrl ?? l.Employee?.PhotoUrl,
                l.Company?.Name ?? "",
                l.Company?.Color ?? "#1e3a8a",
                l.LeaveType?.Name ?? "",
                l.LeaveType?.Icon ?? "🏖",
                l.StartDate,
                l.EndDate,
                (int)(l.EndDate - now).TotalDays + 1
            )).ToList();
        }

        // ═══ Helpers ═══
        private static DateTime GetNextBirthday(DateTime birthDate)
        {
            var now = DateTime.UtcNow;
            var thisYear = new DateTime(now.Year, birthDate.Month, birthDate.Day, 0, 0, 0, DateTimeKind.Utc);
            return thisYear >= now.Date ? thisYear : thisYear.AddYears(1);
        }
    }
}