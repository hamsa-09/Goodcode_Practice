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

        public CourtsControllerTests()
        {
            _courtServiceMock = new Mock<ICourtService>();
            _controller = new CourtsController(_courtServiceMock.Object);

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
        public async Task CreateCourt_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateCourtDto { Name = "New Court" };
            var court = new CourtDto { Id = Guid.NewGuid(), Name = "New Court" };
            _courtServiceMock.Setup(s => s.CreateCourtAsync(dto, It.IsAny<Guid>())).ReturnsAsync(court);

            // Act
            var result = await _controller.CreateCourt(dto);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(CourtsController.GetCourt));
        }
    }
}
