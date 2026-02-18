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
    public class VenuesControllerTests
    {
        private readonly Mock<IVenueService> _venueServiceMock;
        private readonly VenuesController _controller;

        public VenuesControllerTests()
        {
            _venueServiceMock = new Mock<IVenueService>();
            _controller = new VenuesController(_venueServiceMock.Object);

            // Mock User Claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetAllVenues_ReturnsOkWithVenues()
        {
            // Arrange
            var venues = new List<VenueDto> { new VenueDto { Id = Guid.NewGuid(), Name = "Venue 1" } };
            _venueServiceMock.Setup(s => s.GetAllVenuesAsync()).ReturnsAsync(venues);

            // Act
            var result = await _controller.GetAllVenues();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(venues);
        }

        [Fact]
        public async Task GetVenue_ReturnsNotFound_WhenVenueIsNull()
        {
            // Arrange
            _venueServiceMock.Setup(s => s.GetVenueByIdAsync(It.IsAny<Guid>())).ReturnsAsync((VenueDto)null!);

            // Act
            var result = await _controller.GetVenue(Guid.NewGuid());

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateVenue_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateVenueDto { Name = "New Venue" };
            var createdVenue = new VenueDto { Id = Guid.NewGuid(), Name = "New Venue" };
            _venueServiceMock.Setup(s => s.CreateVenueAsync(dto, It.IsAny<Guid>())).ReturnsAsync(createdVenue);

            // Act
            var result = await _controller.CreateVenue(dto);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(VenuesController.GetVenue));
            createdAtActionResult.Value.Should().BeEquivalentTo(createdVenue);
        }
    }
}
