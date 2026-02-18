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
    public class CourtsControllerTests
    {
        private readonly Mock<ICourtService> _courtServiceMock;
        private readonly CourtsController _controller;
        private readonly Guid _userId;

        public CourtsControllerTests()
        {
            _courtServiceMock = new Mock<ICourtService>();
            _controller = new CourtsController(_courtServiceMock.Object);

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
        public async Task GetAllCourts_ReturnsOk()
        {
            // Arrange
            var courts = new List<CourtDto> { new CourtDto { Id = Guid.NewGuid() } };
            _courtServiceMock.Setup(s => s.GetAllCourtsAsync()).ReturnsAsync(courts);

            // Act
            var result = await _controller.GetAllCourts();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(courts);
        }

        [Fact]
        public async Task GetCourt_ReturnsOk_WhenFound()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var court = new CourtDto { Id = courtId };
            _courtServiceMock.Setup(s => s.GetCourtByIdAsync(courtId)).ReturnsAsync(court);

            // Act
            var result = await _controller.GetCourt(courtId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(court);
        }

        [Fact]
        public async Task GetCourt_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            _courtServiceMock.Setup(s => s.GetCourtByIdAsync(courtId)).ReturnsAsync((CourtDto?)null);

            // Act
            var result = await _controller.GetCourt(courtId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetCourtsByVenue_ReturnsOk()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var courts = new List<CourtDto> { new CourtDto { Id = Guid.NewGuid() } };
            _courtServiceMock.Setup(s => s.GetCourtsByVenueIdAsync(venueId)).ReturnsAsync(courts);

            // Act
            var result = await _controller.GetCourtsByVenue(venueId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(courts);
        }

        [Fact]
        public async Task CreateCourt_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateCourtDto { Name = "New Court" };
            var createdCourt = new CourtDto { Id = Guid.NewGuid(), Name = "New Court" };
            _courtServiceMock.Setup(s => s.CreateCourtAsync(dto, _userId)).ReturnsAsync(createdCourt);

            // Act
            var result = await _controller.CreateCourt(dto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(CourtsController.GetCourt));
            createdResult.Value.Should().BeEquivalentTo(createdCourt);
        }

        [Fact]
        public async Task UpdateCourt_ReturnsOk()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var dto = new UpdateCourtDto { Name = "Updated" };
            var updatedCourt = new CourtDto { Id = courtId, Name = "Updated" };
            _courtServiceMock.Setup(s => s.UpdateCourtAsync(courtId, dto, _userId)).ReturnsAsync(updatedCourt);

            // Act
            var result = await _controller.UpdateCourt(courtId, dto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(updatedCourt);
        }

        [Fact]
        public async Task DeleteCourt_ReturnsOk()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            _courtServiceMock.Setup(s => s.DeleteCourtAsync(courtId, _userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCourt(courtId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true });
        }
    }
}
