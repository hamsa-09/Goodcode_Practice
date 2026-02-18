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
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Tests.Controllers
{
    public class VenuesControllerTests
    {
        private readonly Mock<IVenueService> _venueServiceMock;
        private readonly VenuesController _controller;
        private readonly Guid _userId;

        public VenuesControllerTests()
        {
            _venueServiceMock = new Mock<IVenueService>();
            _controller = new VenuesController(_venueServiceMock.Object);

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
        public async Task GetAllVenues_ReturnsOk()
        {
            // Arrange
            var venues = new List<VenueDto> { new VenueDto { Id = Guid.NewGuid() } };
            _venueServiceMock.Setup(s => s.GetAllVenuesAsync()).ReturnsAsync(venues);

            // Act
            var result = await _controller.GetAllVenues();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(venues);
        }

        [Fact]
        public async Task GetVenue_ReturnsOk_WhenFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var venue = new VenueDto { Id = venueId };
            _venueServiceMock.Setup(s => s.GetVenueByIdAsync(venueId)).ReturnsAsync(venue);

            // Act
            var result = await _controller.GetVenue(venueId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(venue);
        }

        [Fact]
        public async Task GetVenue_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            _venueServiceMock.Setup(s => s.GetVenueByIdAsync(venueId)).ReturnsAsync((VenueDto?)null);

            // Act
            var result = await _controller.GetVenue(venueId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetMyVenues_ReturnsOk()
        {
            // Arrange
            var venues = new List<VenueDto> { new VenueDto { Id = Guid.NewGuid() } };
            _venueServiceMock.Setup(s => s.GetVenuesByOwnerAsync(_userId)).ReturnsAsync(venues);

            // Act
            var result = await _controller.GetMyVenues();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(venues);
        }

        [Fact]
        public async Task CreateVenue_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateVenueDto { Name = "New Venue" };
            var createdVenue = new VenueDto { Id = Guid.NewGuid(), Name = "New Venue" };
            _venueServiceMock.Setup(s => s.CreateVenueAsync(dto, _userId)).ReturnsAsync(createdVenue);

            // Act
            var result = await _controller.CreateVenue(dto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(VenuesController.GetVenue));
            createdResult.Value.Should().BeEquivalentTo(createdVenue);
        }

        [Fact]
        public async Task UpdateVenue_ReturnsOk()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var dto = new UpdateVenueDto { Name = "Updated" };
            var updatedVenue = new VenueDto { Id = venueId, Name = "Updated" };
            _venueServiceMock.Setup(s => s.UpdateVenueAsync(venueId, dto, _userId)).ReturnsAsync(updatedVenue);

            // Act
            var result = await _controller.UpdateVenue(venueId, dto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(updatedVenue);
        }

        [Fact]
        public async Task UpdateVenueApproval_ReturnsOk()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var dto = new UpdateVenueApprovalDto { ApprovalStatus = ApprovalStatus.Approved };
            _venueServiceMock.Setup(s => s.UpdateVenueApprovalAsync(venueId, dto)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateVenueApproval(venueId, dto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task DeleteVenue_ReturnsOk()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            _venueServiceMock.Setup(s => s.DeleteVenueAsync(venueId, _userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteVenue(venueId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task GetCurrentUserId_ThrowsUnauthorized_WhenClaimMissing()
        {
            // Arrange - Create controller with no user claims
            var controller = new VenuesController(_venueServiceMock.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            // Act
            Func<Task> act = () => controller.GetMyVenues();

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid user token.");
        }
    }
}
