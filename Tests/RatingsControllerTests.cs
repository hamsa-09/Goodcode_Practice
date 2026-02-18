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

        public RatingsControllerTests()
        {
            _ratingServiceMock = new Mock<IRatingService>();
            _controller = new RatingsController(_ratingServiceMock.Object);

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
        public async Task CreateRating_ReturnsOk()
        {
            // Arrange
            var dto = new CreateRatingDto { Score = 5 };
            var rating = new RatingDto { Id = Guid.NewGuid(), Score = 5 };
            _ratingServiceMock.Setup(s => s.CreateRatingAsync(It.IsAny<Guid>(), dto)).ReturnsAsync(rating);

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
            var ratings = new List<RatingDto> { new RatingDto { Id = Guid.NewGuid() } };
            _ratingServiceMock.Setup(s => s.GetVenueRatingsAsync(It.IsAny<Guid>())).ReturnsAsync(ratings);

            // Act
            var result = await _controller.GetVenueRatings(Guid.NewGuid());

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(ratings);
        }
    }
}
