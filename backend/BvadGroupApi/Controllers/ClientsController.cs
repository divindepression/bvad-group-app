using BvadGroupApi.Dtos;
using BvadGroupApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BvadGroupApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("👤 Clients")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _service;

        public ClientsController(IClientService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? isActive)
        {
            return Ok(await _service.GetAllAsync(new ClientFilters(search, isActive)));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var c = await _service.GetByIdAsync(id);
            return c == null ? NotFound() : Ok(c);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager,Accountant")]
        public async Task<IActionResult> Create([FromBody] CreateClientDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Le nom est obligatoire" });

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created!.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "SuperAdmin,Admin,Director,HR,Manager,Accountant")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateClientDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "SuperAdmin,Admin,Director")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return (await _service.DeleteAsync(id)) ? NoContent() : NotFound();
        }
    }
}