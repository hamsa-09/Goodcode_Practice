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
        private readonly Guid _userId;

        public DiscountsControllerTests()
        {
            _discountServiceMock = new Mock<IDiscountService>();
            _controller = new DiscountsController(_discountServiceMock.Object);

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
        public async Task GetDiscount_ReturnsOk_WhenFound()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            var discount = new DiscountDto { Id = discountId };
            _discountServiceMock.Setup(s => s.GetDiscountByIdAsync(discountId)).ReturnsAsync(discount);

            // Act
            var result = await _controller.GetDiscount(discountId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(discount);
        }

        [Fact]
        public async Task GetDiscount_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            _discountServiceMock.Setup(s => s.GetDiscountByIdAsync(discountId)).ReturnsAsync((DiscountDto?)null);

            // Act
            var result = await _controller.GetDiscount(discountId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetDiscountsByVenue_ReturnsOk()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var discounts = new List<DiscountDto> { new DiscountDto { Id = Guid.NewGuid() } };
            _discountServiceMock.Setup(s => s.GetDiscountsByVenueIdAsync(venueId)).ReturnsAsync(discounts);

            // Act
            var result = await _controller.GetDiscountsByVenue(venueId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(discounts);
        }

        [Fact]
        public async Task GetDiscountsByCourt_ReturnsOk()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var discounts = new List<DiscountDto> { new DiscountDto { Id = Guid.NewGuid() } };
            _discountServiceMock.Setup(s => s.GetDiscountsByCourtIdAsync(courtId)).ReturnsAsync(discounts);

            // Act
            var result = await _controller.GetDiscountsByCourt(courtId);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(discounts);
        }

        [Fact]
        public async Task CreateDiscount_ReturnsCreatedAtAction()
        {
            // Arrange
            var dto = new CreateDiscountDto { PercentOff = 10 };
            var createdDiscount = new DiscountDto { Id = Guid.NewGuid(), PercentOff = 10 };
            _discountServiceMock.Setup(s => s.CreateDiscountAsync(dto, _userId)).ReturnsAsync(createdDiscount);

            // Act
            var result = await _controller.CreateDiscount(dto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(DiscountsController.GetDiscount));
            createdResult.Value.Should().BeEquivalentTo(createdDiscount);
        }

        [Fact]
        public async Task UpdateDiscount_ReturnsOk()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            var dto = new UpdateDiscountDto { PercentOff = 20 };
            var updatedDiscount = new DiscountDto { Id = discountId, PercentOff = 20 };
            _discountServiceMock.Setup(s => s.UpdateDiscountAsync(discountId, dto, _userId)).ReturnsAsync(updatedDiscount);

            // Act
            var result = await _controller.UpdateDiscount(discountId, dto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(updatedDiscount);
        }

        [Fact]
        public async Task DeleteDiscount_ReturnsOk()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            _discountServiceMock.Setup(s => s.DeleteDiscountAsync(discountId, _userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteDiscount(discountId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { success = true });
        }
    }
}
