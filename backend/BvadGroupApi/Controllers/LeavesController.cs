using System.Security.Claims;
using BvadGroupApi.Data;
using BvadGroupApi.Dtos;
using BvadGroupApi.Models;
using BvadGroupApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BvadGroupApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("🏖 Congés")]
    public class LeavesController : ControllerBase
    {
        private readonly ILeaveService _service;
        private readonly AppDbContext _context;

        public LeavesController(ILeaveService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        // ═══════════════════════════════════════
        // 📚 Types de congés
        // ═══════════════════════════════════════
        [HttpGet("types")]
        public async Task<IActionResult> GetTypes()
        {
            return Ok(await _service.GetTypesAsync());
        }

        // ═══════════════════════════════════════
        // 📊 Soldes
        // ═══════════════════════════════════════
        [HttpGet("balances/{employeeId:guid}")]
        public async Task<IActionResult> GetBalances(Guid employeeId, [FromQuery] int? year)
        {
            return Ok(await _service.GetBalancesAsync(employeeId, year));
        }

        // ═══════════════════════════════════════
        // 📝 Demandes de congé
        // ═══════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? companyId,
            [FromQuery] Guid? employeeId,
            [FromQuery] LeaveRequestStatus? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var filters = new LeaveFilters(companyId, employeeId, status, fromDate, toDate);
            return Ok(await _service.GetRequestsAsync(filters));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var r = await _service.GetRequestByIdAsync(id);
            return r == null ? NotFound() : Ok(r);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto dto)
        {
            if (dto.StartDate > dto.EndDate)
                return BadRequest(new { message = "La date de fin doit être postérieure à la date de début" });

            var created = await _service.CreateRequestAsync(dto);
            return created == null
                ? BadRequest(new { message = "Employé ou type de congé introuvable" })
                : CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ═══════════════════════════════════════
        // ✅ Approbation / Refus
        // ═══════════════════════════════════════
        [HttpPost("{id:guid}/approve")]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveLeaveDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var result = await _service.ApproveAsync(id, userId, dto.Comment);
            return result == null
                ? NotFound(new { message = "Demande introuvable ou déjà traitée" })
                : Ok(result);
        }

        [HttpPost("{id:guid}/reject")]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectLeaveDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Comment))
                return BadRequest(new { message = "Un motif de refus est obligatoire" });

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var result = await _service.RejectAsync(id, userId, dto.Comment);
            return result == null
                ? NotFound(new { message = "Demande introuvable ou déjà traitée" })
                : Ok(result);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var ok = await _service.CancelAsync(id, userId);
            return ok ? NoContent() : NotFound();
        }

        // ═══════════════════════════════════════
        // 📅 Calendrier équipe
        // ═══════════════════════════════════════
        [HttpGet("calendar/{companyId:guid}")]
        public async Task<IActionResult> GetCalendar(
            Guid companyId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
            var toDate = to ?? DateTime.UtcNow.AddMonths(3);

            return Ok(await _service.GetCalendarAsync(companyId, fromDate, toDate));
        }
    }
}