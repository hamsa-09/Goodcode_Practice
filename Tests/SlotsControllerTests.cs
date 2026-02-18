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
    public class SlotsControllerTests
    {
        private readonly Mock<ISlotService> _slotServiceMock;
        private readonly SlotsController _controller;
        private readonly Guid _userId;

        public SlotsControllerTests()
        {
            _slotServiceMock = new Mock<ISlotService>();
            _controller = new SlotsController(_slotServiceMock.Object);

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
        public async Task GetAvailableSlots_ReturnsOk()
        {
            // Arrange
            var slots = new List<AvailableSlotDto> { new AvailableSlotDto { Id = Guid.NewGuid() } };
            _slotServiceMock.Setup(s => s.GetAvailableSlotsAsync(null, null, null, null)).ReturnsAsync(slots);

            // Act
            var result = await _controller.GetAvailableSlots();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(slots);
        }

        [Fact]
        public async Task GetSlotDetails_ReturnsOk_WhenFound()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var slot = new AvailableSlotDto { Id = slotId };
            _slotServiceMock.Setup(s => s.GetSlotDetailsAsync(slotId)).ReturnsAsync(slot);

            // Act
            var result = await _controller.GetSlotDetails(slotId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(slot);
        }

        [Fact]
        public async Task GetSlotDetails_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            _slotServiceMock.Setup(s => s.GetSlotDetailsAsync(slotId)).ReturnsAsync((AvailableSlotDto?)null);

            // Act
            var result = await _controller.GetSlotDetails(slotId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task LockSlot_ReturnsOk()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var response = new BookSlotResponseDto { SlotId = slotId };
            _slotServiceMock.Setup(s => s.LockSlotAsync(slotId, _userId)).ReturnsAsync(response);

            // Act
            var result = await _controller.LockSlot(slotId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task ConfirmBooking_ReturnsOk()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var response = new BookSlotResponseDto { SlotId = slotId };
            _slotServiceMock.Setup(s => s.ConfirmBookingAsync(slotId, _userId)).ReturnsAsync(response);

            // Act
            var result = await _controller.ConfirmBooking(slotId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task ReleaseLock_ReturnsOk()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            _slotServiceMock.Setup(s => s.ReleaseLockAsync(slotId, _userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.ReleaseLock(slotId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task CancelBooking_ReturnsOk()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            _slotServiceMock.Setup(s => s.CancelBookingAsync(slotId, _userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.CancelBooking(slotId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true });
        }
    }
}
