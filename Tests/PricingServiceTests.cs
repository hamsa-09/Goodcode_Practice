using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Tests.Services
{
    public class PricingServiceTests
    {
        private readonly Mock<IDemandTrackingService> _demandTrackingServiceMock;
        private readonly Mock<IDiscountRepository> _discountRepositoryMock;
        private readonly Mock<ISlotRepository> _slotRepositoryMock;
        private readonly AppDbContext _dbContext;
        private readonly PricingService _service;

        public PricingServiceTests()
        {
            _demandTrackingServiceMock = new Mock<IDemandTrackingService>();
            _discountRepositoryMock = new Mock<IDiscountRepository>();
            _slotRepositoryMock = new Mock<ISlotRepository>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);

            _service = new PricingService(
                _demandTrackingServiceMock.Object,
                _discountRepositoryMock.Object,
                _slotRepositoryMock.Object,
                _dbContext);
        }

        [Theory]
        [InlineData(0, 1.0)]
        [InlineData(3, 1.2)]
        [InlineData(10, 1.5)]
        public async Task GetDemandMultiplierAsync_ReturnsCorrectMultiplier(int viewerCount, decimal expected)
        {
            // Arrange
            var slotId = Guid.NewGuid();
            _demandTrackingServiceMock.Setup(s => s.GetViewerCountAsync(slotId)).ReturnsAsync(viewerCount);

            // Act
            var result = await _service.GetDemandMultiplierAsync(slotId);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public async Task CalculatePriceAsync_AppliesAllMultipliers()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddHours(2); // Should give 1.5x time multiplier
            _demandTrackingServiceMock.Setup(s => s.GetViewerCountAsync(slotId)).ReturnsAsync(3); // 1.2x demand multiplier
            _discountRepositoryMock.Setup(r => r.GetByVenueIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Discount>());
            _discountRepositoryMock.Setup(r => r.GetByCourtIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Discount>());

            // Act
            var result = await _service.CalculatePriceAsync(slotId, 100, startTime);

            // Assert
            // 100 * 1.2 (demand) * 1.5 (time) = 180
            result.FinalPrice.Should().Be(180);
        }

        [Fact]
        public async Task GetDiscountFactorAsync_ReturnsCorrectFactor_WhenDiscountExists()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddHours(2);
            var discount = new Discount
            {
                IsActive = true,
                PercentOff = 20,
                ValidFrom = DateTime.UtcNow.AddHours(-1),
                ValidTo = DateTime.UtcNow.AddHours(5)
            };
            _discountRepositoryMock.Setup(r => r.GetByVenueIdAsync(venueId)).ReturnsAsync(new List<Discount> { discount });

            // Act
            var result = await _service.GetDiscountFactorAsync(venueId, null, startTime);

            // Assert
            result.Should().Be(0.8m); // 20% off
        }
    }
}
