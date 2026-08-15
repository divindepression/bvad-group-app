using BvadGroupApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BvadGroupApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("📊 Dashboard analytique")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> Overview([FromQuery] Guid? companyId)
        {
            return Ok(await _service.GetOverviewAsync(companyId));
        }

        [HttpGet("employees-by-company")]
        public async Task<IActionResult> EmployeesByCompany()
        {
            return Ok(await _service.GetEmployeesByCompanyAsync());
        }

        [HttpGet("employees-by-department")]
        public async Task<IActionResult> EmployeesByDepartment([FromQuery] Guid? companyId)
        {
            return Ok(await _service.GetEmployeesByDepartmentAsync(companyId));
        }

        [HttpGet("employees-by-contract")]
        public async Task<IActionResult> EmployeesByContract([FromQuery] Guid? companyId)
        {
            return Ok(await _service.GetEmployeesByContractAsync(companyId));
        }

        [HttpGet("hiring-trend")]
        public async Task<IActionResult> HiringTrend([FromQuery] Guid? companyId, [FromQuery] int months = 12)
        {
            return Ok(await _service.GetHiringTrendAsync(companyId, months));
        }

        [HttpGet("leaves-by-month")]
        public async Task<IActionResult> LeavesByMonth([FromQuery] Guid? companyId, [FromQuery] int months = 12)
        {
            return Ok(await _service.GetLeavesByMonthAsync(companyId, months));
        }

        [HttpGet("expiring-contracts")]
        public async Task<IActionResult> ExpiringContracts([FromQuery] Guid? companyId, [FromQuery] int days = 60)
        {
            return Ok(await _service.GetExpiringContractsAsync(companyId, days));
        }

        [HttpGet("upcoming-birthdays")]
        public async Task<IActionResult> UpcomingBirthdays([FromQuery] Guid? companyId, [FromQuery] int days = 30)
        {
            return Ok(await _service.GetUpcomingBirthdaysAsync(companyId, days));
        }

        [HttpGet("absent-today")]
        public async Task<IActionResult> AbsentToday([FromQuery] Guid? companyId)
        {
            return Ok(await _service.GetAbsentTodayAsync(companyId));
        }
    }
}