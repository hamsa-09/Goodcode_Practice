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
        private readonly Mock<IConfiguration> _configurationMock;
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
            _configurationMock = new Mock<IConfiguration>();
            _mapperMock = new Mock<IMapper>();

            _service = new WaitlistService(
                _waitlistRepositoryMock.Object,
                _gameRepositoryMock.Object,
                _userManagerMock.Object,
                _ratingRepositoryMock.Object,
                _gamePlayerRepositoryMock.Object,
                _configurationMock.Object,
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
    }
}
