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
    public class CourtsController : ControllerBase
    {
        private readonly ICourtService _courtService;

        public CourtsController(ICourtService courtService)
        {
            _courtService = courtService;
        }

        /// <summary>
        /// Get all courts.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CourtDto>>> GetAllCourts()
        {
            var courts = await _courtService.GetAllCourtsAsync();
            return Ok(courts);
        }

        /// <summary>
        /// Get court by ID.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CourtDto>> GetCourt(Guid id)
        {
            var court = await _courtService.GetCourtByIdAsync(id);
            if (court == null)
            {
                return NotFound();
            }
            return Ok(court);
        }

        /// <summary>
        /// Get courts by venue ID.
        /// </summary>
        [HttpGet("venue/{venueId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CourtDto>>> GetCourtsByVenue(Guid venueId)
        {
            var courts = await _courtService.GetCourtsByVenueIdAsync(venueId);
            return Ok(courts);
        }

        /// <summary>
        /// Create a new court (venue owner only).
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CourtDto>> CreateCourt([FromBody] CreateCourtDto dto)
        {
            var userId = GetCurrentUserId();
            var court = await _courtService.CreateCourtAsync(dto, userId);
            return CreatedAtAction(nameof(GetCourt), new { id = court.Id }, court);
        }

        /// <summary>
        /// Update a court (venue owner only).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CourtDto>> UpdateCourt(Guid id, [FromBody] UpdateCourtDto dto)
        {
            var userId = GetCurrentUserId();
            var court = await _courtService.UpdateCourtAsync(id, dto, userId);
            return Ok(court);
        }

        /// <summary>
        /// Delete a court (venue owner only).
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourt(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _courtService.DeleteCourtAsync(id, userId);
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
