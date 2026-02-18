using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// Get admin dashboard statistics.
        /// </summary>
        [HttpGet("dashboard/stats")]
        public async Task<ActionResult<AdminStatsDto>> GetStats()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Get all transactions (for audit).
        /// </summary>
        [HttpGet("transactions")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAllTransactions()
        {
            var transactions = await _adminService.GetAllTransactionsAsync();
            return Ok(transactions);
        }

        /// <summary>
        /// Get all venues pending approval.
        /// </summary>
        [HttpGet("venues/pending")]
        public async Task<ActionResult<IEnumerable<VenueDto>>> GetPendingVenues()
        {
            var venues = await _adminService.GetPendingVenuesAsync();
            return Ok(venues);
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new { message = "Admin endpoint reachable." });
        }
    }
}
