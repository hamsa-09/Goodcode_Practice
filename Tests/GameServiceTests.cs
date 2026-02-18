using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Tests.Services
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IGamePlayerRepository> _gamePlayerRepositoryMock;
        private readonly Mock<ISlotRepository> _slotRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GameService _service;

        public GameServiceTests()
        {
            _gameRepositoryMock = new Mock<IGameRepository>();
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _gamePlayerRepositoryMock = new Mock<IGamePlayerRepository>();
            _slotRepositoryMock = new Mock<ISlotRepository>();
            _mapperMock = new Mock<IMapper>();

            _service = new GameService(
                _gameRepositoryMock.Object,
                _userManagerMock.Object,
                _gamePlayerRepositoryMock.Object,
                _slotRepositoryMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task CreateGameAsync_ReturnsGameDto_WhenSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new CreateGameDto { SlotId = Guid.NewGuid(), Type = GameType.Public };
            var game = new Game { Id = Guid.NewGuid(), Type = GameType.Public, Players = new List<GamePlayer>() };

            _mapperMock.Setup(m => m.Map<Game>(dto)).Returns(game);
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(It.IsAny<Guid>())).ReturnsAsync(game);
            _mapperMock.Setup(m => m.Map<GameDto>(game)).Returns(new GameDto { Id = game.Id, Type = game.Type });

            // Act
            var result = await _service.CreateGameAsync(dto, userId);

            // Assert
            result.Should().NotBeNull();
            result.Type.Should().Be(GameType.Public);
            _gameRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Game>()), Times.Once);
            _gamePlayerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<GamePlayer>()), Times.Once);
        }

        [Fact]
        public async Task JoinGameAsync_ThrowsException_WhenGameFull()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Game
            {
                Id = gameId,
                MaxPlayers = 2,
                Status = GameStatus.Pending,
                Players = new List<GamePlayer> { new GamePlayer(), new GamePlayer() }
            };
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(game);

            // Act
            Func<Task> act = () => _service.JoinGameAsync(new JoinGameDto { GameId = gameId }, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Game is full.");
        }

        [Fact]
        public async Task CancelGameAsync_ThrowsUnauthorized_WhenNotCreator()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Game { Id = gameId, CreatedByUserId = Guid.NewGuid() };
            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);

            // Act
            Func<Task> act = () => _service.CancelGameAsync(gameId, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
    }
}
