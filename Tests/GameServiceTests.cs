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
        public async Task GetGameByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync((Game)null);

            // Act
            var result = await _service.GetGameByIdAsync(gameId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetGameByIdAsync_ReturnsMappedGame_WhenFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Game { Id = gameId };
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(game);
            _mapperMock.Setup(m => m.Map<GameDto>(game)).Returns(new GameDto { Id = gameId });

            // Act
            var result = await _service.GetGameByIdAsync(gameId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(gameId);
        }

        [Fact]
        public async Task GetGamesByUserIdAsync_ReturnsMappedGames()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var games = new List<Game> { new Game { Id = gameId } };
            var gameWithPlayers = new Game { Id = gameId, Players = new List<GamePlayer>() };

            _gameRepositoryMock.Setup(r => r.GetByCreatedByUserIdAsync(userId)).ReturnsAsync(games);
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(gameWithPlayers);
            _mapperMock.Setup(m => m.Map<GameDto>(gameWithPlayers)).Returns(new GameDto { Id = gameId });

            // Act
            var result = await _service.GetGamesByUserIdAsync(userId);

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task JoinGameAsync_ThrowsException_WhenGameNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync((Game)null);

            // Act
            Func<Task> act = () => _service.JoinGameAsync(new JoinGameDto { GameId = gameId }, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Game not found.");
        }

        [Fact]
        public async Task JoinGameAsync_ThrowsException_WhenGameNotPending()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Game { Id = gameId, Status = GameStatus.Completed, Players = new List<GamePlayer>() };
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(game);

            // Act
            Func<Task> act = () => _service.JoinGameAsync(new JoinGameDto { GameId = gameId }, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Game is not open for joining.");
        }

        [Fact]
        public async Task JoinGameAsync_ThrowsException_WhenAlreadyInGame()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var game = new Game
            {
                Id = gameId,
                Status = GameStatus.Pending,
                MaxPlayers = 10,
                Players = new List<GamePlayer> { new GamePlayer { UserId = userId } }
            };
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(game);

            // Act
            Func<Task> act = () => _service.JoinGameAsync(new JoinGameDto { GameId = gameId }, userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You are already in this game.");
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
        public async Task JoinGameAsync_Succeeds_WhenValid()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var game = new Game
            {
                Id = gameId,
                Status = GameStatus.Pending,
                MaxPlayers = 10,
                Players = new List<GamePlayer>()
            };
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(game);
            _mapperMock.Setup(m => m.Map<GameDto>(game)).Returns(new GameDto { Id = gameId });

            // Act
            var result = await _service.JoinGameAsync(new JoinGameDto { GameId = gameId }, userId);

            // Assert
            result.Should().NotBeNull();
            _gamePlayerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<GamePlayer>()), Times.Once);
        }

        [Fact]
        public async Task LeaveGameAsync_ThrowsException_WhenGameNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync((Game)null);

            // Act
            Func<Task> act = () => _service.LeaveGameAsync(new LeaveGameDto { GameId = gameId }, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Game not found.");
        }

        [Fact]
        public async Task LeaveGameAsync_ThrowsException_WhenGameNotPending()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Game { Id = gameId, Status = GameStatus.Completed, Players = new List<GamePlayer>() };
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(game);

            // Act
            Func<Task> act = () => _service.LeaveGameAsync(new LeaveGameDto { GameId = gameId }, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot leave a game that is not pending.");
        }

        [Fact]
        public async Task LeaveGameAsync_ThrowsException_WhenNotInGame()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var game = new Game { Id = gameId, Status = GameStatus.Pending, Players = new List<GamePlayer>() };
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(game);

            // Act
            Func<Task> act = () => _service.LeaveGameAsync(new LeaveGameDto { GameId = gameId }, userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You are not in this game.");
        }

        [Fact]
        public async Task LeaveGameAsync_ThrowsException_WhenCreatorLeavesWithOtherPlayers()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            var game = new Game
            {
                Id = gameId,
                Status = GameStatus.Pending,
                CreatedByUserId = creatorId,
                Players = new List<GamePlayer>
                {
                    new GamePlayer { UserId = creatorId },
                    new GamePlayer { UserId = Guid.NewGuid() }
                }
            };
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(game);

            // Act
            Func<Task> act = () => _service.LeaveGameAsync(new LeaveGameDto { GameId = gameId }, creatorId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Game creator cannot leave if other players have joined.");
        }

        [Fact]
        public async Task LeaveGameAsync_Succeeds_WhenValid()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var player = new GamePlayer { UserId = userId };
            var game = new Game
            {
                Id = gameId,
                Status = GameStatus.Pending,
                CreatedByUserId = Guid.NewGuid(), // Different from userId
                Players = new List<GamePlayer> { player }
            };
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(game);

            // Act
            var result = await _service.LeaveGameAsync(new LeaveGameDto { GameId = gameId }, userId);

            // Assert
            result.Should().BeTrue();
            _gamePlayerRepositoryMock.Verify(r => r.RemoveAsync(player), Times.Once);
        }

        [Fact]
        public async Task CancelGameAsync_ThrowsException_WhenGameNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game)null);

            // Act
            Func<Task> act = () => _service.CancelGameAsync(gameId, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Game not found.");
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

        [Fact]
        public async Task CancelGameAsync_Succeeds_WhenCreator()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            var game = new Game { Id = gameId, CreatedByUserId = creatorId };
            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);

            // Act
            var result = await _service.CancelGameAsync(gameId, creatorId);

            // Assert
            result.Should().BeTrue();
            game.Status.Should().Be(GameStatus.Cancelled);
            _gameRepositoryMock.Verify(r => r.UpdateAsync(game), Times.Once);
        }

        [Fact]
        public async Task GetPendingGamesWithLowPlayersAsync_ReturnsGames()
        {
            // Arrange
            var games = new List<Game> { new Game(), new Game() };
            _gameRepositoryMock.Setup(r => r.GetPendingGamesWithLowPlayersAsync(2)).ReturnsAsync(games);

            // Act
            var result = await _service.GetPendingGamesWithLowPlayersAsync(2);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task CancelGamesWithLowPlayersAsync_CancelsGamesStartingSoon()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var slotId = Guid.NewGuid();
            var game = new Game { Id = gameId, SlotId = slotId, MinPlayers = 3 };
            var gameWithPlayers = new Game
            {
                Id = gameId,
                SlotId = slotId,
                MinPlayers = 3,
                Players = new List<GamePlayer> { new GamePlayer() } // Only 1 player, needs 3
            };
            var slot = new Slot { Id = slotId, StartTime = DateTime.UtcNow.AddMinutes(30) }; // Starts in 30 min

            _gameRepositoryMock.Setup(r => r.GetPendingGamesWithLowPlayersAsync(0)).ReturnsAsync(new List<Game> { game });
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(gameWithPlayers);
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);

            // Act
            await _service.CancelGamesWithLowPlayersAsync();

            // Assert
            gameWithPlayers.Status.Should().Be(GameStatus.Cancelled);
            _gameRepositoryMock.Verify(r => r.UpdateAsync(gameWithPlayers), Times.Once);
        }
    }
}
