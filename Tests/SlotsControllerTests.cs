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
    public class SlotsControllerTests
    {
        private readonly Mock<ISlotService> _slotServiceMock;
        private readonly SlotsController _controller;

        public SlotsControllerTests()
        {
            _slotServiceMock = new Mock<ISlotService>();
            _controller = new SlotsController(_slotServiceMock.Object);

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
        public async Task GetAvailableSlots_ReturnsOk()
        {
            // Arrange
            var slots = new List<AvailableSlotDto> { new AvailableSlotDto() };
            _slotServiceMock.Setup(s => s.GetAvailableSlotsAsync(null, null, null, null)).ReturnsAsync(slots);

            // Act
            var result = await _controller.GetAvailableSlots();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(slots);
        }

        [Fact]
        public async Task LockSlot_ReturnsOk()
        {
            // Arrange
            var response = new BookSlotResponseDto { Status = SlotStatus.Locked };
            _slotServiceMock.Setup(s => s.LockSlotAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(response);

            // Act
            var result = await _controller.LockSlot(Guid.NewGuid());

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }
    }
}
