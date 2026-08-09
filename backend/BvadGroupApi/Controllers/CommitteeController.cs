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
    [Tags("🏛 Comité de direction")]
    public class CommitteeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommitteeController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Membres du comité d'une filiale</summary>
        [HttpGet("{companyId:guid}")]
        public async Task<IActionResult> GetCommittee(Guid companyId)
        {
            var members = await _context.Employees
                .Include(e => e.Company)
                .Include(e => e.Manager)
                .Where(e => e.CompanyId == companyId && e.IsCommitteeMember)
                .OrderBy(e => e.CommitteePosition)
                .ThenBy(e => e.LastName)
                .ToListAsync();

            var dtos = members.Select(e => new EmployeeDto(
                e.Id, e.FirstName, e.LastName, e.MiddleName, e.FullName,
                e.Email, e.PhoneNumber, e.Position, e.Department,
                e.Status.ToString(), e.ContractType.ToString(),
                e.HireDate, e.EndDate, e.Salary, e.BirthDate, e.Age,
                e.Gender.ToString(), e.City, e.Country, e.PhotoUrl,
                e.CompanyId, e.Company?.Name ?? "", e.Company?.Color ?? "#1e3a8a",
                e.Company?.Logo, e.CompanyRole.ToString(),
                e.IsCommitteeMember, e.CommitteePosition.ToString(),
                e.CommitteePositionCustom, e.ManagerId, e.Manager?.FullName,
                e.UserId, e.CreatedAt
            )).ToList();

            return Ok(dtos);
        }
    }
}