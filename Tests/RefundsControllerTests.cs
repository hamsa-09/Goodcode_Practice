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
    public class RefundsControllerTests
    {
        private readonly Mock<IRefundService> _refundServiceMock;
        private readonly RefundsController _controller;

        public RefundsControllerTests()
        {
            _refundServiceMock = new Mock<IRefundService>();
            _controller = new RefundsController(_refundServiceMock.Object);

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
        public async Task RequestRefund_ReturnsOk()
        {
            // Arrange
            var dto = new RequestRefundDto { SlotId = Guid.NewGuid() };
            var refund = new RefundDto { Id = Guid.NewGuid() };
            _refundServiceMock.Setup(s => s.RequestRefundAsync(It.IsAny<Guid>(), dto)).ReturnsAsync(refund);

            // Act
            var result = await _controller.RequestRefund(dto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(refund);
        }

        [Fact]
        public async Task GetUserRefunds_ReturnsOk()
        {
            // Arrange
            var refunds = new List<RefundDto> { new RefundDto { Id = Guid.NewGuid() } };
            _refundServiceMock.Setup(s => s.GetUserRefundsAsync(It.IsAny<Guid>())).ReturnsAsync(refunds);

            // Act
            var result = await _controller.GetUserRefunds();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(refunds);
        }
    }
}
