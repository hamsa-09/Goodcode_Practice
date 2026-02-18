using System;
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
    public class PaymentsControllerTests
    {
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly PaymentsController _controller;
        private readonly Guid _userId;

        public PaymentsControllerTests()
        {
            _paymentServiceMock = new Mock<IPaymentService>();
            _controller = new PaymentsController(_paymentServiceMock.Object);

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
        public async Task ProcessPayment_ReturnsOk()
        {
            // Arrange
            var dto = new PaymentDto { SlotId = Guid.NewGuid() };
            var response = new PaymentResponseDto { Amount = 100 };
            _paymentServiceMock.Setup(s => s.ProcessPaymentAsync(_userId, dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.ProcessPayment(dto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }
    }
}
