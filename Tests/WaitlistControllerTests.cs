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
    public class WaitlistControllerTests
    {
        private readonly Mock<IWaitlistService> _waitlistServiceMock;
        private readonly WaitlistController _controller;

        public WaitlistControllerTests()
        {
            _waitlistServiceMock = new Mock<IWaitlistService>();
            _controller = new WaitlistController(_waitlistServiceMock.Object);

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
        public async Task JoinWaitlist_ReturnsOk()
        {
            // Arrange
            var dto = new JoinWaitlistDto { GameId = Guid.NewGuid() };
            var waitlist = new WaitlistDto { Id = Guid.NewGuid() };
            _waitlistServiceMock.Setup(s => s.JoinWaitlistAsync(dto.GameId, It.IsAny<Guid>())).ReturnsAsync(waitlist);

            // Act
            var result = await _controller.JoinWaitlist(dto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(waitlist);
        }

        [Fact]
        public async Task GetWaitlistByGame_ReturnsOk()
        {
            // Arrange
            var waitlist = new List<WaitlistDto> { new WaitlistDto { Id = Guid.NewGuid() } };
            _waitlistServiceMock.Setup(s => s.GetWaitlistByGameAsync(It.IsAny<Guid>())).ReturnsAsync(waitlist);

            // Act
            var result = await _controller.GetWaitlistByGame(Guid.NewGuid());

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(waitlist);
        }
    }
}
