using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Services
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAllAsync(EmployeeFilters filters);
        Task<EmployeeDto?> GetByIdAsync(Guid id);
        Task<EmployeeDto?> CreateAsync(CreateEmployeeDto dto);
        Task<EmployeeDto?> UpdateAsync(Guid id, UpdateEmployeeDto dto);
        Task<bool> DeleteAsync(Guid id);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;
        private readonly IUserProvisioningService _userProvisioning;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(
            AppDbContext context,
            IUserProvisioningService userProvisioning,
            ILogger<EmployeeService> logger)
        {
            _context = context;
            _userProvisioning = userProvisioning;
            _logger = logger;
        }

        public async Task<List<EmployeeDto>> GetAllAsync(EmployeeFilters filters)
        {
            var query = _context.Employees
                .Include(e => e.Company)
                .Include(e => e.Manager)
                .AsQueryable();

            if (filters.CompanyId.HasValue)
                query = query.Where(e => e.CompanyId == filters.CompanyId);

            if (filters.Status.HasValue)
                query = query.Where(e => e.Status == filters.Status);

            if (!string.IsNullOrWhiteSpace(filters.Department))
                query = query.Where(e => e.Department == filters.Department);

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var s = filters.Search.ToLower();
                query = query.Where(e =>
                    e.FirstName.ToLower().Contains(s) ||
                    e.LastName.ToLower().Contains(s) ||
                    e.Email.ToLower().Contains(s) ||
                    e.Position.ToLower().Contains(s));
            }

            var list = await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return list.Select(ToDto).ToList();
        }

        public async Task<EmployeeDto?> GetByIdAsync(Guid id)
        {
            var emp = await _context.Employees
                .Include(e => e.Company)
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.Id == id);

            return emp == null ? null : ToDto(emp);
        }

        public async Task<EmployeeDto?> CreateAsync(CreateEmployeeDto dto)
        {
            var company = await _context.Companies.FindAsync(dto.CompanyId);
            if (company == null) return null;

            var emp = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                MiddleName = dto.MiddleName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Position = dto.Position,
                Department = dto.Department,
                Gender = dto.Gender,
                BirthDate = dto.BirthDate?.ToUniversalTime(),
                HireDate = dto.HireDate.ToUniversalTime(),
                EndDate = dto.EndDate?.ToUniversalTime(),
                ContractType = dto.ContractType,
                Salary = dto.Salary,
                Status = dto.Status,
                City = dto.City,
                Country = dto.Country,
                CompanyId = dto.CompanyId,
                PhotoUrl = dto.PhotoUrl,
                Notes = dto.Notes,
                CompanyRole = dto.CompanyRole,
                IsCommitteeMember = dto.IsCommitteeMember,
                CommitteePosition = dto.CommitteePosition,
                CommitteePositionCustom = dto.CommitteePositionCustom,
                ManagerId = dto.ManagerId
            };

            // 🆔 Générer matricule automatiquement
            emp.EmployeeNumber = await GenerateEmployeeNumberAsync(company);

            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            // 🎯 Auto-création du compte User
            try
            {
                var user = await _userProvisioning.CreateUserForEmployeeAsync(emp);
                emp.UserId = user.Id;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Impossible de créer le compte User pour {Name}", emp.FullName);
            }

            emp.Company = company;
            return ToDto(emp);
        }

        public async Task<EmployeeDto?> UpdateAsync(Guid id, UpdateEmployeeDto dto)
        {
            var emp = await _context.Employees
                .Include(e => e.Company)
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emp == null) return null;

            emp.FirstName = dto.FirstName;
            emp.LastName = dto.LastName;
            emp.MiddleName = dto.MiddleName;
            emp.Email = dto.Email;
            emp.PhoneNumber = dto.PhoneNumber;
            emp.Position = dto.Position;
            emp.Department = dto.Department;
            emp.Gender = dto.Gender;
            emp.BirthDate = dto.BirthDate?.ToUniversalTime();
            emp.HireDate = dto.HireDate.ToUniversalTime();
            emp.EndDate = dto.EndDate?.ToUniversalTime();
            emp.ContractType = dto.ContractType;
            emp.Salary = dto.Salary;
            emp.Status = dto.Status;
            emp.City = dto.City;
            emp.Country = dto.Country;
            emp.CompanyId = dto.CompanyId;
            emp.PhotoUrl = dto.PhotoUrl;
            emp.Notes = dto.Notes;
            emp.CompanyRole = dto.CompanyRole;
            emp.IsCommitteeMember = dto.IsCommitteeMember;
            emp.CommitteePosition = dto.CommitteePosition;
            emp.CommitteePositionCustom = dto.CommitteePositionCustom;
            emp.ManagerId = dto.ManagerId;
            emp.UpdatedAt = DateTime.UtcNow;

            if (emp.CompanyId != dto.CompanyId)
            {
                var newCompany = await _context.Companies.FindAsync(dto.CompanyId);
                if (newCompany != null) emp.Company = newCompany;
            }

            await _context.SaveChangesAsync();
            return ToDto(emp);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return false;

            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string> GenerateEmployeeNumberAsync(Company company)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"{company.Code}-{year}-";

            var count = await _context.Employees
                .CountAsync(e => e.CompanyId == company.Id
                             && e.EmployeeNumber != null
                             && e.EmployeeNumber.StartsWith(prefix));

            return $"{prefix}{(count + 1):D3}";
        }

        // ==============================
        private static EmployeeDto ToDto(Employee e) =>
            new(
                e.Id,
                e.FirstName,
                e.LastName,
                e.MiddleName,
                e.FullName,
                e.Email,
                e.PhoneNumber,
                e.Position,
                e.Department,
                e.Status.ToString(),
                e.ContractType.ToString(),
                e.HireDate,
                e.EndDate,
                e.Salary,
                e.BirthDate,
                e.Age,
                e.Gender.ToString(),
                e.City,
                e.Country,
                e.PhotoUrl,
                e.CompanyId,
                e.Company?.Name ?? "",
                e.Company?.Color ?? "#1e3a8a",
                e.Company?.Logo,
                e.CompanyRole.ToString(),
                e.IsCommitteeMember,
                e.CommitteePosition.ToString(),
                e.CommitteePositionCustom,
                e.ManagerId,
                e.Manager?.FullName,
                e.UserId,
                e.CreatedAt,

        // 🆕 NOUVEAUX
        e.EmployeeNumber,
        e.IdentityPhotoUrl,
        e.SignatureUrl,
        e.BankName,
        e.BankAccountNumber,
        e.MobileMoneyOperator,
        e.MobileMoneyNumber,
        e.EmergencyContactName,
        e.EmergencyContactPhone,
        e.EmergencyContactRelation,
        e.NationalIdNumber,
        e.SocialSecurityNumber

            );
    }
}