using System.Security.Claims;
using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("👤 Mon compte")]
    public class MeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Chercher l'employé lié
            var emp = await _context.Employees
                .Include(e => e.Company)
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            EmployeeDto? empDto = null;
            if (emp != null)
            {
                empDto = new EmployeeDto(
                    emp.Id,
                    emp.FirstName,
                    emp.LastName,
                    emp.MiddleName,
                    emp.FullName,
                    emp.Email,
                    emp.PhoneNumber,
                    emp.Position,
                    emp.Department,
                    emp.Status.ToString(),
                    emp.ContractType.ToString(),
                    emp.HireDate,
                    emp.EndDate,
                    emp.Salary,
                    emp.BirthDate,
                    emp.Age,
                    emp.Gender.ToString(),
                    emp.City,
                    emp.Country,
                    emp.PhotoUrl,
                    emp.CompanyId,
                    emp.Company?.Name ?? "",
                    emp.Company?.Color ?? "#1e3a8a",
                    emp.Company?.Logo,
                    emp.CompanyRole.ToString(),
                    emp.IsCommitteeMember,
                    emp.CommitteePosition.ToString(),
                    emp.CommitteePositionCustom,
                    emp.ManagerId,
                    emp.Manager?.FullName,
                    emp.UserId,
                    emp.CreatedAt,

                    // 🆕 NOUVEAUX CHAMPS
                    emp.EmployeeNumber,
                    emp.IdentityPhotoUrl,
                    emp.SignatureUrl,
                    emp.BankName,
                    emp.BankAccountNumber,
                    emp.MobileMoneyOperator,
                    emp.MobileMoneyNumber,
                    emp.EmergencyContactName,
                    emp.EmergencyContactPhone,
                    emp.EmergencyContactRelation,
                    emp.NationalIdNumber,
                    emp.SocialSecurityNumber
                );
            }

            return Ok(new MyProfileDto(
                user.Id,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName,
                user.FullName,
                user.Role.ToString(),
                user.PhotoUrl,
                empDto
            ));
        }
    }
}