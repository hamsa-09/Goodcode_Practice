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
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Get game by ID.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<GameDto>> GetGame(Guid id)
        {
            var game = await _gameService.GetGameByIdAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            return Ok(game);
        }

        /// <summary>
        /// Get games created by the current user.
        /// </summary>
        [HttpGet("my-games")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetMyGames()
        {
            var userId = GetCurrentUserId();
            var games = await _gameService.GetGamesByUserIdAsync(userId);
            return Ok(games);
        }

        /// <summary>
        /// Create a new game.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<GameDto>> CreateGame([FromBody] CreateGameDto dto)
        {
            var userId = GetCurrentUserId();
            var game = await _gameService.CreateGameAsync(dto, userId);
            return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game);
        }

        /// <summary>
        /// Join a game.
        /// </summary>
        [HttpPost("join")]
        public async Task<ActionResult<GameDto>> JoinGame([FromBody] JoinGameDto dto)
        {
            var userId = GetCurrentUserId();
            var game = await _gameService.JoinGameAsync(dto, userId);
            return Ok(game);
        }

        /// <summary>
        /// Leave a game.
        /// </summary>
        [HttpPost("leave")]
        public async Task<IActionResult> LeaveGame([FromBody] LeaveGameDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _gameService.LeaveGameAsync(dto, userId);
            return Ok(new { success = result });
        }

        /// <summary>
        /// Cancel a game (creator only).
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelGame(Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _gameService.CancelGameAsync(id, userId);
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
