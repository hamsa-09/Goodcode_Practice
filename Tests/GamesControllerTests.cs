using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Controllers;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Tests.Controllers
{
    public class GamesControllerTests
    {
        private readonly Mock<IGameService> _gameServiceMock;
        private readonly GamesController _controller;
        private readonly Guid _userId;

        public GamesControllerTests()
        {
            _gameServiceMock = new Mock<IGameService>();
            _controller = new GamesController(_gameServiceMock.Object);

            _userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetGame_ReturnsOk_WhenFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new GameDto { Id = gameId };
            _gameServiceMock.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);

            // Act
            var result = await _controller.GetGame(gameId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(game);
        }

        [Fact]
        public async Task GetGame_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _gameServiceMock.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync((GameDto?)null);

            // Act
            var result = await _controller.GetGame(gameId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetMyGames_ReturnsOk()
        {
            // Arrange
            var games = new List<GameDto> { new GameDto { Id = Guid.NewGuid() } };
            _gameServiceMock.Setup(s => s.GetGamesByUserIdAsync(_userId)).ReturnsAsync(games);

            // Act
            var result = await _controller.GetMyGames();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(games);
        }

        [Fact]
        public async Task CreateGame_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateGameDto { SlotId = Guid.NewGuid() };
            var createdGame = new GameDto { Id = Guid.NewGuid(), SlotId = dto.SlotId };
            _gameServiceMock.Setup(s => s.CreateGameAsync(dto, _userId)).ReturnsAsync(createdGame);

            // Act
            var result = await _controller.CreateGame(dto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(GamesController.GetGame));
            createdResult.Value.Should().BeEquivalentTo(createdGame);
        }

        [Fact]
        public async Task JoinGame_ReturnsOk()
        {
            // Arrange
            var dto = new JoinGameDto { GameId = Guid.NewGuid() };
            var game = new GameDto { Id = dto.GameId };
            _gameServiceMock.Setup(s => s.JoinGameAsync(dto, _userId)).ReturnsAsync(game);

            // Act
            var result = await _controller.JoinGame(dto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(game);
        }

        [Fact]
        public async Task LeaveGame_ReturnsOk()
        {
            // Arrange
            var dto = new LeaveGameDto { GameId = Guid.NewGuid() };
            _gameServiceMock.Setup(s => s.LeaveGameAsync(dto, _userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.LeaveGame(dto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task CancelGame_ReturnsOk()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _gameServiceMock.Setup(s => s.CancelGameAsync(gameId, _userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.CancelGame(gameId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true });
        }
    }
}
