using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Controllers;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
        }

        [Fact]
        public async Task Register_ReturnsOk()
        {
            // Arrange
            var dto = new RegisterRequestDto { Email = "test@test.com" };
            var response = new AuthResponseDto { AccessToken = "fake-token" };
            _authServiceMock.Setup(s => s.RegisterAsync(dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Login_ReturnsOk()
        {
            // Arrange
            var dto = new LoginRequestDto { Email = "admin@test.com" };
            var response = new AuthResponseDto { AccessToken = "admin-token" };
            _authServiceMock.Setup(s => s.LoginAsync(dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }
    }
}
