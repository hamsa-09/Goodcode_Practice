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
    public class RefundsController : ControllerBase
    {
        private readonly IRefundService _refundService;

        public RefundsController(IRefundService refundService)
        {
            _refundService = refundService;
        }

        /// <summary>
        /// Request a refund for a booked slot.
        /// </summary>
        [HttpPost("request")]
        public async Task<ActionResult<RefundDto>> RequestRefund([FromBody] RequestRefundDto dto)
        {
            var userId = GetCurrentUserId();
            var refund = await _refundService.RequestRefundAsync(userId, dto);
            return Ok(refund);
        }

        /// <summary>
        /// Get user's refund history.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RefundDto>>> GetUserRefunds()
        {
            var userId = GetCurrentUserId();
            var refunds = await _refundService.GetUserRefundsAsync(userId);
            return Ok(refunds);
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
