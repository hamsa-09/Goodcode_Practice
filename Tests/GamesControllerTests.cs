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

        public GamesControllerTests()
        {
            _gameServiceMock = new Mock<IGameService>();
            _controller = new GamesController(_gameServiceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetGame_ReturnsNotFound_WhenNull()
        {
            // Arrange
            _gameServiceMock.Setup(s => s.GetGameByIdAsync(It.IsAny<Guid>())).ReturnsAsync((GameDto)null!);

            // Act
            var result = await _controller.GetGame(Guid.NewGuid());

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateGame_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateGameDto { SlotId = Guid.NewGuid() };
            var game = new GameDto { Id = Guid.NewGuid() };
            _gameServiceMock.Setup(s => s.CreateGameAsync(dto, It.IsAny<Guid>())).ReturnsAsync(game);

            // Act
            var result = await _controller.CreateGame(dto);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(GamesController.GetGame));
        }
    }
}
