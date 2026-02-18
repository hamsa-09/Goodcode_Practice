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
    public class VenuesController : ControllerBase
    {
        private readonly IVenueService _venueService;

        public VenuesController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        /// <summary>
        /// Get all venues.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VenueDto>>> GetAllVenues()
        {
            var venues = await _venueService.GetAllVenuesAsync();
            return Ok(venues);
        }

        /// <summary>
        /// Get venue by ID.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<VenueDto>> GetVenue(Guid id)
        {
            var venue = await _venueService.GetVenueByIdAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return Ok(venue);
        }

        /// <summary>
        /// Get venues owned by the current user.
        /// </summary>
        [HttpGet("my-venues")]
        public async Task<ActionResult<IEnumerable<VenueDto>>> GetMyVenues()
        {
            var userId = GetCurrentUserId();
            var venues = await _venueService.GetVenuesByOwnerAsync(userId);
            return Ok(venues);
        }

        /// <summary>
        /// Create a new venue.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<VenueDto>> CreateVenue([FromBody] CreateVenueDto dto)
        {
            var userId = GetCurrentUserId();
            var venue = await _venueService.CreateVenueAsync(dto, userId);
            return CreatedAtAction(nameof(GetVenue), new { id = venue.Id }, venue);
        }

        /// <summary>
        /// Update a venue (owner only).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<VenueDto>> UpdateVenue(Guid id, [FromBody] UpdateVenueDto dto)
        {
            var userId = GetCurrentUserId();
            var venue = await _venueService.UpdateVenueAsync(id, dto, userId);
            return Ok(venue);
        }

        /// <summary>
        /// Update venue approval status (Admin only).
        /// </summary>
        [HttpPut("{id}/approval")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateVenueApproval(Guid id, [FromBody] UpdateVenueApprovalDto dto)
        {
            var result = await _venueService.UpdateVenueApprovalAsync(id, dto);
            return Ok(new { success = result });
        }

        /// <summary>
        /// Delete a venue (owner only).
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenue(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _venueService.DeleteVenueAsync(id, userId);
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
