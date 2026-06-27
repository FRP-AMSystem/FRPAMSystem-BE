using System.Security.Claims;
using FRPAMSystem.BusinessTier.Constants;
using FRPAMSystem.BusinessTier.Payload.Notification;
using FRPAMSystem.BusinessTier.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FRPAMSystem_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ViewMine(
            [FromQuery] NotificationFilter filter,
            [FromQuery] PagingModel pagingModel)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var result = await _service.ViewByUserAsync(userId.Value, filter, pagingModel);

            return Ok(new
            {
                success = true,
                message = "Get notifications successfully",
                data = result
            });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var count = await _service.GetUnreadCountAsync(userId.Value);

            return Ok(new
            {
                success = true,
                message = "Get unread notification count successfully",
                data = new { unreadCount = count }
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var result = await _service.GetByIdForUserAsync(id, userId.Value);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Notification not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Get notification successfully",
                data = result
            });
        }

        [HttpPatch("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var result = await _service.MarkAsReadAsync(id, userId.Value);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Notification not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Mark notification as read successfully"
            });
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var updatedCount = await _service.MarkAllAsReadAsync(userId.Value);

            return Ok(new
            {
                success = true,
                message = "Mark all notifications as read successfully",
                data = new { updatedCount }
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var userId = GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new { success = false, message = "Invalid user token" });
            }

            var result = await _service.SoftDeleteAsync(id, userId.Value);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Notification not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Delete notification successfully"
            });
        }

        private int? GetCurrentUserId()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdValue, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}
