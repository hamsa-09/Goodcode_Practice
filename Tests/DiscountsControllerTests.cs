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
    public class DiscountsControllerTests
    {
        private readonly Mock<IDiscountService> _discountServiceMock;
        private readonly DiscountsController _controller;

        public DiscountsControllerTests()
        {
            _discountServiceMock = new Mock<IDiscountService>();
            _controller = new DiscountsController(_discountServiceMock.Object);

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
        public async Task GetDiscountsByVenue_ReturnsOk()
        {
            // Arrange
            var discounts = new List<DiscountDto> { new DiscountDto { Id = Guid.NewGuid() } };
            _discountServiceMock.Setup(s => s.GetDiscountsByVenueIdAsync(It.IsAny<Guid>())).ReturnsAsync(discounts);

            // Act
            var result = await _controller.GetDiscountsByVenue(Guid.NewGuid());

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(discounts);
        }

        [Fact]
        public async Task CreateDiscount_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateDiscountDto { PercentOff = 10 };
            var discount = new DiscountDto { Id = Guid.NewGuid(), PercentOff = 10 };
            _discountServiceMock.Setup(s => s.CreateDiscountAsync(dto, It.IsAny<Guid>())).ReturnsAsync(discount);

            // Act
            var result = await _controller.CreateDiscount(dto);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdAtActionResult.ActionName.Should().Be(nameof(DiscountsController.GetDiscount));
        }
    }
}
