using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlotsController : ControllerBase
    {
        private readonly ISlotService _slotService;

        public SlotsController(ISlotService slotService)
        {
            _slotService = slotService;
        }

        /// <summary>
        /// Get available slots with dynamic pricing.
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AvailableSlotDto>>> GetAvailableSlots(
            [FromQuery] Guid? courtId = null,
            [FromQuery] Guid? venueId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var slots = await _slotService.GetAvailableSlotsAsync(courtId, venueId, startDate, endDate);
            return Ok(slots);
        }

        /// <summary>
        /// Get slot details by ID.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AvailableSlotDto>> GetSlotDetails(Guid id)
        {
            var slot = await _slotService.GetSlotDetailsAsync(id);
            if (slot == null)
            {
                return NotFound();
            }
            return Ok(slot);
        }

        /// <summary>
        /// Lock a slot for booking (reserves price for 5 minutes).
        /// </summary>
        [HttpPost("{id}/lock")]
        [Authorize]
        public async Task<ActionResult<BookSlotResponseDto>> LockSlot(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _slotService.LockSlotAsync(id, userId);
            return Ok(result);
        }

        /// <summary>
        /// Confirm booking after locking (completes the booking).
        /// </summary>
        [HttpPost("{id}/confirm")]
        [Authorize]
        public async Task<ActionResult<BookSlotResponseDto>> ConfirmBooking(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _slotService.ConfirmBookingAsync(id, userId);
            return Ok(result);
        }

        /// <summary>
        /// Release a locked slot (cancels the lock).
        /// </summary>
        [HttpPost("{id}/release")]
        [Authorize]
        public async Task<IActionResult> ReleaseLock(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _slotService.ReleaseLockAsync(id, userId);
            return Ok(new { success = result });
        }

        /// <summary>
        /// Cancel a booked slot.
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _slotService.CancelBookingAsync(id, userId);
            return Ok(new { success = result });
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user token.");
            }
            return userId;
        }
    }
}
