using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Tests.Services
{
    public class WaitlistServiceTests
    {
        private readonly Mock<IWaitlistRepository> _waitlistRepositoryMock;
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IRatingRepository> _ratingRepositoryMock;
        private readonly Mock<IGamePlayerRepository> _gamePlayerRepositoryMock;
        private readonly IConfiguration _configuration;
        private readonly Mock<IMapper> _mapperMock;
        private readonly WaitlistService _service;

        public WaitlistServiceTests()
        {
            _waitlistRepositoryMock = new Mock<IWaitlistRepository>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _ratingRepositoryMock = new Mock<IRatingRepository>();
            _gamePlayerRepositoryMock = new Mock<IGamePlayerRepository>();
            _mapperMock = new Mock<IMapper>();

            // Use a real ConfigurationBuilder to avoid NullReferenceException with GetValue<T>
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Waitlist:MaxSize", "20" }
                })
                .Build();

            _service = new WaitlistService(
                _waitlistRepositoryMock.Object,
                _gameRepositoryMock.Object,
                _userManagerMock.Object,
                _ratingRepositoryMock.Object,
                _gamePlayerRepositoryMock.Object,
                _configuration,
                _mapperMock.Object);
        }

        [Fact]
        public async Task JoinWaitlistAsync_ReturnsMappedWaitlist_WhenSuccessful()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var game = new Game { Id = gameId, Status = Enums.GameStatus.Pending };
            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(new Game { Players = new List<GamePlayer>() });
            _waitlistRepositoryMock.Setup(r => r.GetByGameAndUserAsync(gameId, userId)).ReturnsAsync((Waitlist)null);
            _waitlistRepositoryMock.Setup(r => r.GetCountByGameIdAsync(gameId)).ReturnsAsync(0);
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(new User { AggregatedRating = 4.5m });

            var waitlist = new Waitlist { Id = Guid.NewGuid() };
            _waitlistRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(waitlist);
            _mapperMock.Setup(m => m.Map<WaitlistDto>(waitlist)).Returns(new WaitlistDto());

            // Act
            var result = await _service.JoinWaitlistAsync(gameId, userId);

            // Assert
            result.Should().NotBeNull();
            _waitlistRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Waitlist>()), Times.Once);
        }

        [Fact]
        public async Task JoinWaitlistAsync_ThrowsException_WhenGameNotFound()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game)null);

            // Act
            Func<Task> act = () => _service.JoinWaitlistAsync(gameId, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Game not found.");
        }

        [Fact]
        public async Task JoinWaitlistAsync_ThrowsException_WhenGameNotPending()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Game { Id = gameId, Status = Enums.GameStatus.Completed };
            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);

            // Act
            Func<Task> act = () => _service.JoinWaitlistAsync(gameId, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Can only join waitlist for pending games.");
        }

        [Fact]
        public async Task JoinWaitlistAsync_ThrowsException_WhenAlreadyInGame()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var game = new Game { Id = gameId, Status = Enums.GameStatus.Pending };
            var gameWithPlayers = new Game
            {
                Id = gameId,
                Players = new List<GamePlayer> { new GamePlayer { UserId = userId } }
            };
            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(gameWithPlayers);

            // Act
            Func<Task> act = () => _service.JoinWaitlistAsync(gameId, userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You are already in this game.");
        }

        [Fact]
        public async Task JoinWaitlistAsync_ReturnsExisting_WhenAlreadyInWaitlist()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var game = new Game { Id = gameId, Status = Enums.GameStatus.Pending };
            var existingWaitlist = new Waitlist { Id = Guid.NewGuid(), GameId = gameId, UserId = userId };

            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(new Game { Players = new List<GamePlayer>() });
            _waitlistRepositoryMock.Setup(r => r.GetByGameAndUserAsync(gameId, userId)).ReturnsAsync(existingWaitlist);
            _mapperMock.Setup(m => m.Map<WaitlistDto>(existingWaitlist)).Returns(new WaitlistDto());

            // Act
            var result = await _service.JoinWaitlistAsync(gameId, userId);

            // Assert
            result.Should().NotBeNull();
            _waitlistRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Waitlist>()), Times.Never);
        }

        [Fact]
        public async Task JoinWaitlistAsync_ThrowsException_WhenWaitlistFull()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var game = new Game { Id = gameId, Status = Enums.GameStatus.Pending };

            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(new Game { Players = new List<GamePlayer>() });
            _waitlistRepositoryMock.Setup(r => r.GetByGameAndUserAsync(gameId, userId)).ReturnsAsync((Waitlist)null);
            _waitlistRepositoryMock.Setup(r => r.GetCountByGameIdAsync(gameId)).ReturnsAsync(20); // At max size

            // Act
            Func<Task> act = () => _service.JoinWaitlistAsync(gameId, userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Waitlist is full*");
        }

        [Fact]
        public async Task InviteFromWaitlistAsync_Works_WhenCreatorInvites()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            var invitedId = Guid.NewGuid();
            var game = new Game { Id = gameId, CreatedByUserId = creatorId, MaxPlayers = 10, Players = new List<GamePlayer>() };

            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(game);
            _waitlistRepositoryMock.Setup(r => r.GetByGameAndUserAsync(gameId, invitedId)).ReturnsAsync(new Waitlist());

            // Act
            var result = await _service.InviteFromWaitlistAsync(gameId, creatorId, invitedId);

            // Assert
            result.Should().BeTrue();
            _gamePlayerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<GamePlayer>()), Times.Once);
            _waitlistRepositoryMock.Verify(r => r.RemoveAsync(It.IsAny<Waitlist>()), Times.Once);
        }

        [Fact]
        public async Task InviteFromWaitlistAsync_ThrowsUnauthorized_WhenNotCreator()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var game = new Game { Id = gameId, CreatedByUserId = creatorId };

            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);

            // Act
            Func<Task> act = () => _service.InviteFromWaitlistAsync(gameId, otherUserId, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task InviteFromWaitlistAsync_ThrowsException_WhenGameFull()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            var invitedId = Guid.NewGuid();
            var game = new Game
            {
                Id = gameId,
                CreatedByUserId = creatorId,
                MaxPlayers = 2,
                Players = new List<GamePlayer> { new GamePlayer(), new GamePlayer() }
            };

            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(game);

            // Act
            Func<Task> act = () => _service.InviteFromWaitlistAsync(gameId, creatorId, invitedId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Game is full.");
        }

        [Fact]
        public async Task InviteFromWaitlistAsync_ThrowsException_WhenUserNotInWaitlist()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            var invitedId = Guid.NewGuid();
            var game = new Game { Id = gameId, CreatedByUserId = creatorId, MaxPlayers = 10, Players = new List<GamePlayer>() };

            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(game);
            _waitlistRepositoryMock.Setup(r => r.GetByGameAndUserAsync(gameId, invitedId)).ReturnsAsync((Waitlist)null);

            // Act
            Func<Task> act = () => _service.InviteFromWaitlistAsync(gameId, creatorId, invitedId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("User is not in the waitlist.");
        }

        [Fact]
        public async Task LeaveWaitlistAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var waitlist = new Waitlist { Id = Guid.NewGuid(), GameId = gameId, UserId = userId };
            _waitlistRepositoryMock.Setup(r => r.GetByGameAndUserAsync(gameId, userId)).ReturnsAsync(waitlist);

            // Act
            var result = await _service.LeaveWaitlistAsync(gameId, userId);

            // Assert
            result.Should().BeTrue();
            _waitlistRepositoryMock.Verify(r => r.RemoveAsync(waitlist), Times.Once);
        }

        [Fact]
        public async Task LeaveWaitlistAsync_ReturnsFalse_WhenNotInWaitlist()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _waitlistRepositoryMock.Setup(r => r.GetByGameAndUserAsync(gameId, userId)).ReturnsAsync((Waitlist)null);

            // Act
            var result = await _service.LeaveWaitlistAsync(gameId, userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetWaitlistByGameAsync_ReturnsMappedList()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var waitlists = new List<Waitlist> { new Waitlist(), new Waitlist() };
            _waitlistRepositoryMock.Setup(r => r.GetByGameIdAsync(gameId)).ReturnsAsync(waitlists);
            _mapperMock.Setup(m => m.Map<IEnumerable<WaitlistDto>>(waitlists)).Returns(new List<WaitlistDto> { new WaitlistDto(), new WaitlistDto() });

            // Act
            var result = await _service.GetWaitlistByGameAsync(gameId);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetWaitlistCountAsync_ReturnsCount()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _waitlistRepositoryMock.Setup(r => r.GetCountByGameIdAsync(gameId)).ReturnsAsync(5);

            // Act
            var result = await _service.GetWaitlistCountAsync(gameId);

            // Assert
            result.Should().Be(5);
        }

        [Fact]
        public async Task RemoveWaitlistForGameAsync_RemovesAll()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var waitlists = new List<Waitlist> { new Waitlist(), new Waitlist() };
            _waitlistRepositoryMock.Setup(r => r.GetByGameIdAsync(gameId)).ReturnsAsync(waitlists);

            // Act
            await _service.RemoveWaitlistForGameAsync(gameId);

            // Assert
            _waitlistRepositoryMock.Verify(r => r.RemoveAsync(It.IsAny<Waitlist>()), Times.Exactly(2));
            _waitlistRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
