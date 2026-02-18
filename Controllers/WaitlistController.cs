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
    [Authorize]
    public class WaitlistController : ControllerBase
    {
        private readonly IWaitlistService _waitlistService;

        public WaitlistController(IWaitlistService waitlistService)
        {
            _waitlistService = waitlistService;
        }

        /// <summary>
        /// Join waitlist for a game.
        /// </summary>
        [HttpPost("join")]
        public async Task<ActionResult<WaitlistDto>> JoinWaitlist([FromBody] JoinWaitlistDto dto)
        {
            var userId = GetCurrentUserId();
            var waitlist = await _waitlistService.JoinWaitlistAsync(dto.GameId, userId);
            return Ok(waitlist);
        }

        /// <summary>
        /// Leave waitlist for a game.
        /// </summary>
        [HttpPost("leave")]
        public async Task<IActionResult> LeaveWaitlist([FromBody] JoinWaitlistDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _waitlistService.LeaveWaitlistAsync(dto.GameId, userId);
            return Ok(new { success = result });
        }

        /// <summary>
        /// Get waitlist for a game (sorted by priority).
        /// </summary>
        [HttpGet("game/{gameId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<WaitlistDto>>> GetWaitlistByGame(Guid gameId)
        {
            var waitlist = await _waitlistService.GetWaitlistByGameAsync(gameId);
            return Ok(waitlist);
        }

        /// <summary>
        /// Invite a user from waitlist to join the game (game creator only).
        /// </summary>
        [HttpPost("invite")]
        public async Task<IActionResult> InviteFromWaitlist([FromBody] InviteFromWaitlistDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _waitlistService.InviteFromWaitlistAsync(dto.GameId, userId, dto.UserId);
            return Ok(new { success = result });
        }

        /// <summary>
        /// Get waitlist count for a game.
        /// </summary>
        [HttpGet("game/{gameId}/count")]
        [AllowAnonymous]
        public async Task<ActionResult<int>> GetWaitlistCount(Guid gameId)
        {
            var count = await _waitlistService.GetWaitlistCountAsync(gameId);
            return Ok(new { count });
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
