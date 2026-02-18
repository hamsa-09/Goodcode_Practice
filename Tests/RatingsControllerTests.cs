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
    public class RatingsControllerTests
    {
        private readonly Mock<IRatingService> _ratingServiceMock;
        private readonly RatingsController _controller;
        private readonly Guid _userId;

        public RatingsControllerTests()
        {
            _ratingServiceMock = new Mock<IRatingService>();
            _controller = new RatingsController(_ratingServiceMock.Object);

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
        public async Task CreateRating_ReturnsOk()
        {
            // Arrange
            var dto = new CreateRatingDto { Score = 5, Comment = "Great" };
            var rating = new RatingDto { Id = Guid.NewGuid(), Score = 5 };
            _ratingServiceMock.Setup(s => s.CreateRatingAsync(_userId, dto)).ReturnsAsync(rating);

            // Act
            var result = await _controller.CreateRating(dto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(rating);
        }

        [Fact]
        public async Task GetVenueRatings_ReturnsOk()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var ratings = new List<RatingDto> { new RatingDto { Id = Guid.NewGuid() } };
            _ratingServiceMock.Setup(s => s.GetVenueRatingsAsync(venueId)).ReturnsAsync(ratings);

            // Act
            var result = await _controller.GetVenueRatings(venueId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(ratings);
        }

        [Fact]
        public async Task GetCourtRatings_ReturnsOk()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var ratings = new List<RatingDto> { new RatingDto { Id = Guid.NewGuid() } };
            _ratingServiceMock.Setup(s => s.GetCourtRatingsAsync(courtId)).ReturnsAsync(ratings);

            // Act
            var result = await _controller.GetCourtRatings(courtId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(ratings);
        }

        [Fact]
        public async Task GetPlayerRatings_ReturnsOk()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            var ratings = new List<RatingDto> { new RatingDto { Id = Guid.NewGuid() } };
            _ratingServiceMock.Setup(s => s.GetPlayerRatingsAsync(playerId)).ReturnsAsync(ratings);

            // Act
            var result = await _controller.GetPlayerRatings(playerId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(ratings);
        }

        [Fact]
        public async Task GetPlayerProfile_ReturnsOk()
        {
            // Arrange
            var playerId = Guid.NewGuid();
            var profile = new PlayerProfileDto { UserId = playerId };
            _ratingServiceMock.Setup(s => s.GetPlayerProfileAsync(playerId)).ReturnsAsync(profile);

            // Act
            var result = await _controller.GetPlayerProfile(playerId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(profile);
        }

        [Fact]
        public async Task GetVenueRatingSummary_ReturnsOk()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var summary = new VenueRatingSummaryDto { AverageRating = 4.5m };
            _ratingServiceMock.Setup(s => s.GetVenueRatingSummaryAsync(venueId)).ReturnsAsync(summary);

            // Act
            var result = await _controller.GetVenueRatingSummary(venueId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(summary);
        }

        [Fact]
        public async Task GetCourtRatingSummary_ReturnsOk()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var summary = new CourtRatingSummaryDto { AverageRating = 4.2m };
            _ratingServiceMock.Setup(s => s.GetCourtRatingSummaryAsync(courtId)).ReturnsAsync(summary);

            // Act
            var result = await _controller.GetCourtRatingSummary(courtId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(summary);
        }
    }
}
