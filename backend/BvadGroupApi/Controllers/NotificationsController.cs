using System.Security.Claims;
using BvadGroupApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BvadGroupApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Tags("🔔 Notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        private Guid? CurrentUserId
        {
            get
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(idStr, out var g) ? g : null;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int take = 50)
        {
            if (CurrentUserId is not Guid userId) return Unauthorized();
            return Ok(await _service.GetUserNotificationsAsync(userId, take));
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            if (CurrentUserId is not Guid userId) return Unauthorized();
            return Ok(await _service.GetCountAsync(userId));
        }

        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            if (CurrentUserId is not Guid userId) return Unauthorized();
            await _service.MarkAsReadAsync(userId, id);
            return NoContent();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (CurrentUserId is not Guid userId) return Unauthorized();
            await _service.MarkAllAsReadAsync(userId);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (CurrentUserId is not Guid userId) return Unauthorized();
            await _service.DeleteAsync(userId, id);
            return NoContent();
        }
    }
}