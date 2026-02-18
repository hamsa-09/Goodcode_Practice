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
        private readonly Guid _userId;

        public WaitlistControllerTests()
        {
            _waitlistServiceMock = new Mock<IWaitlistService>();
            _controller = new WaitlistController(_waitlistServiceMock.Object);

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
        public async Task JoinWaitlist_ReturnsOk()
        {
            // Arrange
            var dto = new JoinWaitlistDto { GameId = Guid.NewGuid() };
            var waitlist = new WaitlistDto { Id = Guid.NewGuid() };
            _waitlistServiceMock.Setup(s => s.JoinWaitlistAsync(dto.GameId, _userId)).ReturnsAsync(waitlist);

            // Act
            var result = await _controller.JoinWaitlist(dto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(waitlist);
        }

        [Fact]
        public async Task LeaveWaitlist_ReturnsOk()
        {
            // Arrange
            var dto = new JoinWaitlistDto { GameId = Guid.NewGuid() };
            _waitlistServiceMock.Setup(s => s.LeaveWaitlistAsync(dto.GameId, _userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.LeaveWaitlist(dto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task GetWaitlistByGame_ReturnsOk()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var waitlist = new List<WaitlistDto> { new WaitlistDto { Id = Guid.NewGuid() } };
            _waitlistServiceMock.Setup(s => s.GetWaitlistByGameAsync(gameId)).ReturnsAsync(waitlist);

            // Act
            var result = await _controller.GetWaitlistByGame(gameId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(waitlist);
        }

        [Fact]
        public async Task InviteFromWaitlist_ReturnsOk()
        {
            // Arrange
            var dto = new InviteFromWaitlistDto { GameId = Guid.NewGuid(), UserId = Guid.NewGuid() };
            _waitlistServiceMock.Setup(s => s.InviteFromWaitlistAsync(dto.GameId, _userId, dto.UserId)).ReturnsAsync(true);

            // Act
            var result = await _controller.InviteFromWaitlist(dto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task GetWaitlistCount_ReturnsOk()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _waitlistServiceMock.Setup(s => s.GetWaitlistCountAsync(gameId)).ReturnsAsync(5);

            // Act
            var result = await _controller.GetWaitlistCount(gameId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { count = 5 });
        }
    }
}
