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
    }
}
