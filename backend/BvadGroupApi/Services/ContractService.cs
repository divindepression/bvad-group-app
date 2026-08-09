using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    public interface IContractService
    {
        Task<List<ContractDto>> GetAllAsync(ContractFilters filters);
        Task<ContractDto?> GetByIdAsync(Guid id);
        Task<ContractDto?> CreateAsync(CreateContractDto dto, Guid? createdByUserId);
        Task<ContractDto?> UpdateAsync(Guid id, UpdateContractDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<Contract?> GetContractEntityAsync(Guid id);
    }

    public class ContractService : IContractService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ContractService> _logger;

        public ContractService(AppDbContext context, ILogger<ContractService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ContractDto>> GetAllAsync(ContractFilters filters)
        {
            var query = _context.Contracts
                .Include(c => c.Employee)
                .Include(c => c.Company)
                .AsQueryable();

            if (filters.CompanyId.HasValue)
                query = query.Where(c => c.CompanyId == filters.CompanyId);

            if (filters.EmployeeId.HasValue)
                query = query.Where(c => c.EmployeeId == filters.EmployeeId);

            if (filters.Status.HasValue)
                query = query.Where(c => c.Status == filters.Status);

            if (filters.Type.HasValue)
                query = query.Where(c => c.ContractType == filters.Type);

            var list = await query
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            var dtos = list.Select(ToDto).ToList();

            if (filters.ExpiringSoon == true)
                dtos = dtos.Where(d => d.IsExpiringSoon).ToList();

            return dtos;
        }

        public async Task<ContractDto?> GetByIdAsync(Guid id)
        {
            var c = await _context.Contracts
                .Include(x => x.Employee)
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.Id == id);

            return c == null ? null : ToDto(c);
        }

        public async Task<Contract?> GetContractEntityAsync(Guid id)
        {
            return await _context.Contracts
                .Include(x => x.Employee)
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ContractDto?> CreateAsync(CreateContractDto dto, Guid? createdByUserId)
        {
            var employee = await _context.Employees
                .Include(e => e.Company)
                .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId);

            if (employee == null) return null;

            // Générer le numéro automatiquement
            var contractNumber = await GenerateContractNumberAsync(employee.Company);

            var contract = new Contract
            {
                EmployeeId = employee.Id,
                CompanyId = employee.CompanyId,
                ContractNumber = contractNumber,
                ContractType = dto.ContractType,
                Status = dto.Status,
                Position = dto.Position,
                Department = dto.Department,
                StartDate = dto.StartDate.ToUniversalTime(),
                EndDate = dto.EndDate?.ToUniversalTime(),
                SignedDate = dto.SignedDate?.ToUniversalTime(),
                Salary = dto.Salary,
                Currency = dto.Currency,
                TrialPeriodMonths = dto.TrialPeriodMonths,
                WeeklyHours = dto.WeeklyHours,
                SpecialClauses = dto.SpecialClauses,
                Notes = dto.Notes,
                CreatedById = createdByUserId
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            contract.Employee = employee;
            contract.Company = employee.Company;

            return ToDto(contract);
        }

        public async Task<ContractDto?> UpdateAsync(Guid id, UpdateContractDto dto)
        {
            var c = await _context.Contracts
                .Include(x => x.Employee)
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c == null) return null;

            c.ContractType = dto.ContractType;
            c.Position = dto.Position;
            c.Department = dto.Department;
            c.StartDate = dto.StartDate.ToUniversalTime();
            c.EndDate = dto.EndDate?.ToUniversalTime();
            c.SignedDate = dto.SignedDate?.ToUniversalTime();
            c.Salary = dto.Salary;
            c.Currency = dto.Currency;
            c.TrialPeriodMonths = dto.TrialPeriodMonths;
            c.WeeklyHours = dto.WeeklyHours;
            c.SpecialClauses = dto.SpecialClauses;
            c.Notes = dto.Notes;
            c.Status = dto.Status;
            c.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ToDto(c);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var c = await _context.Contracts.FindAsync(id);
            if (c == null) return false;

            _context.Contracts.Remove(c);
            await _context.SaveChangesAsync();
            return true;
        }

        // ======================================================
        private async Task<string> GenerateContractNumberAsync(Company company)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"{company.Code}-{year}-";

            var count = await _context.Contracts
                .CountAsync(c => c.CompanyId == company.Id && c.ContractNumber.StartsWith(prefix));

            return $"{prefix}{(count + 1):D4}";  // ex: BVAD_TECH-2025-0001
        }

        private static ContractDto ToDto(Contract c) =>
            new(
                c.Id,
                c.EmployeeId,
                c.Employee?.FullName ?? "",
                c.Employee?.Position,
                c.CompanyId,
                c.Company?.Name ?? "",
                c.Company?.Color ?? "#1e3a8a",
                c.Company?.Logo,
                c.ContractNumber,
                c.ContractType.ToString(),
                c.Status.ToString(),
                c.Position,
                c.Department,
                c.StartDate,
                c.EndDate,
                c.SignedDate,
                c.Salary,
                c.Currency,
                c.TrialPeriodMonths,
                c.WeeklyHours,
                c.DocumentUrl,
                c.DocumentFileName,
                c.DocumentSize,
                c.SpecialClauses,
                c.Notes,
                c.RemainingDays,
                c.IsExpiringSoon,
                c.IsExpired,
                c.CreatedAt
            );
    }
}