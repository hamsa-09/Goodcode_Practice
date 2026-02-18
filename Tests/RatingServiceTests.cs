using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Tests.Services
{
    public class RatingServiceTests
    {
        private readonly Mock<IRatingRepository> _ratingRepositoryMock;
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IVenueRepository> _venueRepositoryMock;
        private readonly Mock<ICourtRepository> _courtRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AppDbContext _dbContext;
        private readonly RatingService _service;

        public RatingServiceTests()
        {
            _ratingRepositoryMock = new Mock<IRatingRepository>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _venueRepositoryMock = new Mock<IVenueRepository>();
            _courtRepositoryMock = new Mock<ICourtRepository>();
            _mapperMock = new Mock<IMapper>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);

            _service = new RatingService(
                _ratingRepositoryMock.Object,
                _gameRepositoryMock.Object,
                _userManagerMock.Object,
                _venueRepositoryMock.Object,
                _courtRepositoryMock.Object,
                _dbContext,
                _mapperMock.Object);
        }

        [Fact]
        public async Task CreateRatingAsync_ThrowsException_WhenScoreInvalid()
        {
            // Arrange
            var dto = new CreateRatingDto { Score = 6 };

            // Act
            Func<Task> act = () => _service.CreateRatingAsync(Guid.NewGuid(), dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Rating score must be between 1 and 5.");
        }

        [Fact]
        public async Task CreateRatingAsync_ThrowsException_WhenScoreTooLow()
        {
            // Arrange
            var dto = new CreateRatingDto { Score = 0 };

            // Act
            Func<Task> act = () => _service.CreateRatingAsync(Guid.NewGuid(), dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Rating score must be between 1 and 5.");
        }

        [Fact]
        public async Task CreateRatingAsync_Succeeds_ForVenue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var venueId = Guid.NewGuid();
            var dto = new CreateRatingDto { Type = RatingType.Venue, VenueId = venueId, Score = 4 };
            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(new Venue());

            var rating = new Rating { Id = Guid.NewGuid() };
            _ratingRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rating);
            _mapperMock.Setup(m => m.Map<RatingDto>(rating)).Returns(new RatingDto());

            // Act
            var result = await _service.CreateRatingAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            _ratingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Rating>()), Times.Once);
        }

        [Fact]
        public async Task CreateRatingAsync_ThrowsException_WhenVenueIdMissing()
        {
            // Arrange
            var dto = new CreateRatingDto { Type = RatingType.Venue, Score = 4 }; // No VenueId

            // Act
            Func<Task> act = () => _service.CreateRatingAsync(Guid.NewGuid(), dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("VenueId is required for venue ratings.");
        }

        [Fact]
        public async Task CreateRatingAsync_ThrowsException_WhenVenueNotFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var dto = new CreateRatingDto { Type = RatingType.Venue, VenueId = venueId, Score = 4 };
            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync((Venue)null);

            // Act
            Func<Task> act = () => _service.CreateRatingAsync(Guid.NewGuid(), dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Venue not found.");
        }

        [Fact]
        public async Task CreateRatingAsync_Succeeds_ForCourt()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var dto = new CreateRatingDto { Type = RatingType.Court, CourtId = courtId, Score = 3 };
            _courtRepositoryMock.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync(new Court());

            var rating = new Rating { Id = Guid.NewGuid() };
            _ratingRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(rating);
            _mapperMock.Setup(m => m.Map<RatingDto>(rating)).Returns(new RatingDto());

            // Act
            var result = await _service.CreateRatingAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateRatingAsync_ThrowsException_WhenCourtIdMissing()
        {
            // Arrange
            var dto = new CreateRatingDto { Type = RatingType.Court, Score = 4 }; // No CourtId

            // Act
            Func<Task> act = () => _service.CreateRatingAsync(Guid.NewGuid(), dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("CourtId is required for court ratings.");
        }

        [Fact]
        public async Task CreateRatingAsync_ThrowsException_WhenCourtNotFound()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var dto = new CreateRatingDto { Type = RatingType.Court, CourtId = courtId, Score = 4 };
            _courtRepositoryMock.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync((Court)null);

            // Act
            Func<Task> act = () => _service.CreateRatingAsync(Guid.NewGuid(), dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Court not found.");
        }

        [Fact]
        public async Task CreateRatingAsync_ThrowsException_WhenPlayerIdOrGameIdMissing()
        {
            // Arrange
            var dto = new CreateRatingDto { Type = RatingType.Player, Score = 4 }; // No PlayerId or GameId

            // Act
            Func<Task> act = () => _service.CreateRatingAsync(Guid.NewGuid(), dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("PlayerId and GameId are required for player ratings.");
        }

        [Fact]
        public async Task CreateRatingAsync_ThrowsException_WhenGameNotFound_ForPlayerRating()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var dto = new CreateRatingDto { Type = RatingType.Player, PlayerId = Guid.NewGuid(), GameId = gameId, Score = 4 };
            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Game)null);

            // Act
            Func<Task> act = () => _service.CreateRatingAsync(Guid.NewGuid(), dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Game not found.");
        }

        [Fact]
        public async Task CreateRatingAsync_ThrowsException_WhenGameNotCompleted()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var dto = new CreateRatingDto { Type = RatingType.Player, PlayerId = Guid.NewGuid(), GameId = gameId, Score = 4 };
            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(new Game { Status = GameStatus.Pending });

            // Act
            Func<Task> act = () => _service.CreateRatingAsync(Guid.NewGuid(), dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Can only rate players after game completion.");
        }

        [Fact]
        public async Task CreateRatingAsync_ThrowsUnauthorized_WhenUserNotInGame()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var dto = new CreateRatingDto { Type = RatingType.Player, PlayerId = Guid.NewGuid(), GameId = gameId, Score = 4 };

            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(new Game { Status = GameStatus.Completed });
            _gameRepositoryMock.Setup(r => r.GetByIdWithPlayersAsync(gameId)).ReturnsAsync(new Game
            {
                Players = new List<GamePlayer>() // User not in game
            });

            // Act
            Func<Task> act = () => _service.CreateRatingAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task GetVenueRatingsAsync_ReturnsMappedList()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var ratings = new List<Rating> { new Rating(), new Rating() };
            _ratingRepositoryMock.Setup(r => r.GetByVenueIdAsync(venueId)).ReturnsAsync(ratings);
            _mapperMock.Setup(m => m.Map<IEnumerable<RatingDto>>(ratings)).Returns(new List<RatingDto> { new RatingDto(), new RatingDto() });

            // Act
            var result = await _service.GetVenueRatingsAsync(venueId);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetCourtRatingsAsync_ReturnsMappedList()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var ratings = new List<Rating> { new Rating() };
            _ratingRepositoryMock.Setup(r => r.GetByCourtIdAsync(courtId)).ReturnsAsync(ratings);
            _mapperMock.Setup(m => m.Map<IEnumerable<RatingDto>>(ratings)).Returns(new List<RatingDto> { new RatingDto() });

            // Act
            var result = await _service.GetCourtRatingsAsync(courtId);

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetPlayerRatingsAsync_ReturnsMappedList()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            var ratings = new List<Rating> { new Rating() };
            _ratingRepositoryMock.Setup(r => r.GetByPlayerIdAsync(playerId)).ReturnsAsync(ratings);
            _mapperMock.Setup(m => m.Map<IEnumerable<RatingDto>>(ratings)).Returns(new List<RatingDto> { new RatingDto() });

            // Act
            var result = await _service.GetPlayerRatingsAsync(playerId);

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetPlayerProfileAsync_ThrowsException_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync((User)null);

            // Act
            Func<Task> act = () => _service.GetPlayerProfileAsync(userId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("User not found.");
        }

        [Fact]
        public async Task GetPlayerProfileAsync_ReturnsProfile_WhenUserFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, UserName = "testuser", AggregatedRating = 4.5m };
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
            _ratingRepositoryMock.Setup(r => r.GetByPlayerIdAsync(userId)).ReturnsAsync(new List<Rating>());

            // Act
            var result = await _service.GetPlayerProfileAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.UserName.Should().Be("testuser");
            result.AggregatedRating.Should().Be(4.5m);
        }

        [Fact]
        public async Task GetVenueRatingSummaryAsync_ReturnsSummary()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var ratings = new List<Rating>
            {
                new Rating { Score = 4 },
                new Rating { Score = 5 }
            };
            _ratingRepositoryMock.Setup(r => r.GetByVenueIdAsync(venueId)).ReturnsAsync(ratings);
            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(new Venue { Name = "Test Venue" });

            // Act
            var result = await _service.GetVenueRatingSummaryAsync(venueId);

            // Assert
            result.AverageRating.Should().Be(4.5m);
            result.TotalRatings.Should().Be(2);
        }

        [Fact]
        public async Task GetVenueRatingSummaryAsync_ReturnsZero_WhenNoRatings()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            _ratingRepositoryMock.Setup(r => r.GetByVenueIdAsync(venueId)).ReturnsAsync(new List<Rating>());
            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(new Venue { Name = "Empty Venue" });

            // Act
            var result = await _service.GetVenueRatingSummaryAsync(venueId);

            // Assert
            result.AverageRating.Should().Be(0);
            result.TotalRatings.Should().Be(0);
        }

        [Fact]
        public async Task GetCourtRatingSummaryAsync_ReturnsSummary()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var ratings = new List<Rating>
            {
                new Rating { Score = 3 },
                new Rating { Score = 5 }
            };
            _ratingRepositoryMock.Setup(r => r.GetByCourtIdAsync(courtId)).ReturnsAsync(ratings);
            _courtRepositoryMock.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync(new Court { Name = "Test Court" });

            // Act
            var result = await _service.GetCourtRatingSummaryAsync(courtId);

            // Assert
            result.AverageRating.Should().Be(4.0m);
            result.TotalRatings.Should().Be(2);
        }

        [Fact]
        public async Task GetCourtRatingSummaryAsync_ReturnsZero_WhenNoRatings()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            _ratingRepositoryMock.Setup(r => r.GetByCourtIdAsync(courtId)).ReturnsAsync(new List<Rating>());
            _courtRepositoryMock.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync(new Court { Name = "Empty Court" });

            // Act
            var result = await _service.GetCourtRatingSummaryAsync(courtId);

            // Assert
            result.AverageRating.Should().Be(0);
            result.TotalRatings.Should().Be(0);
        }

        [Fact]
        public async Task UpdatePlayerAggregatedRatingAsync_DoesNothing_WhenNoRatings()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            _ratingRepositoryMock.Setup(r => r.GetByPlayerIdAsync(playerId)).ReturnsAsync(new List<Rating>());

            // Act
            await _service.UpdatePlayerAggregatedRatingAsync(playerId);

            // Assert
            _userManagerMock.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePlayerAggregatedRatingAsync_UpdatesRating_WhenRatingsExist()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            var ratings = new List<Rating> { new Rating { Score = 4 }, new Rating { Score = 5 } };
            var user = new User { Id = playerId };

            _ratingRepositoryMock.Setup(r => r.GetByPlayerIdAsync(playerId)).ReturnsAsync(ratings);
            _userManagerMock.Setup(m => m.FindByIdAsync(playerId.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            await _service.UpdatePlayerAggregatedRatingAsync(playerId);

            // Assert
            user.AggregatedRating.Should().Be(4.5m);
            _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
        }
    }
}
