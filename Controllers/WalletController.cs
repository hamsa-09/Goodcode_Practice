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
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        /// <summary>
        /// Get current user's wallet balance.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<WalletDto>> GetWallet()
        {
            var userId = GetCurrentUserId();
            var wallet = await _walletService.GetWalletAsync(userId);
            return Ok(wallet);
        }

        /// <summary>
        /// Add funds to wallet (idempotent).
        /// </summary>
        [HttpPost("add-funds")]
        public async Task<ActionResult<TransactionDto>> AddFunds([FromBody] AddFundsDto dto)
        {
            var userId = GetCurrentUserId();
            var transaction = await _walletService.AddFundsAsync(userId, dto);
            return Ok(transaction);
        }

        /// <summary>
        /// Get transaction history.
        /// </summary>
        [HttpGet("transactions")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionHistory()
        {
            var userId = GetCurrentUserId();
            var transactions = await _walletService.GetTransactionHistoryAsync(userId);
            return Ok(transactions);
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
