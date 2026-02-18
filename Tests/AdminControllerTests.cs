using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Controllers;

namespace Assignment_Example_HU.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _controller = new AdminController();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "AdminUser"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public void HealthCheck_ReturnsOk()
        {
            // Act
            var result = _controller.HealthCheck();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }
    }
}
