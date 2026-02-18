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
    public class DiscountsController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountsController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        /// <summary>
        /// Get discount by ID.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<DiscountDto>> GetDiscount(Guid id)
        {
            var discount = await _discountService.GetDiscountByIdAsync(id);
            if (discount == null)
            {
                return NotFound();
            }
            return Ok(discount);
        }

        /// <summary>
        /// Get discounts by venue ID.
        /// </summary>
        [HttpGet("venue/{venueId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DiscountDto>>> GetDiscountsByVenue(Guid venueId)
        {
            var discounts = await _discountService.GetDiscountsByVenueIdAsync(venueId);
            return Ok(discounts);
        }

        /// <summary>
        /// Get discounts by court ID.
        /// </summary>
        [HttpGet("court/{courtId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DiscountDto>>> GetDiscountsByCourt(Guid courtId)
        {
            var discounts = await _discountService.GetDiscountsByCourtIdAsync(courtId);
            return Ok(discounts);
        }

        /// <summary>
        /// Create a new discount (venue/court owner only).
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DiscountDto>> CreateDiscount([FromBody] CreateDiscountDto dto)
        {
            var userId = GetCurrentUserId();
            var discount = await _discountService.CreateDiscountAsync(dto, userId);
            return CreatedAtAction(nameof(GetDiscount), new { id = discount.Id }, discount);
        }

        /// <summary>
        /// Update a discount (venue/court owner only).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<DiscountDto>> UpdateDiscount(Guid id, [FromBody] UpdateDiscountDto dto)
        {
            var userId = GetCurrentUserId();
            var discount = await _discountService.UpdateDiscountAsync(id, dto, userId);
            return Ok(discount);
        }

        /// <summary>
        /// Delete a discount (venue/court owner only).
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscount(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _discountService.DeleteDiscountAsync(id, userId);
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
