using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;
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

        // ─── GetDemandMultiplierAsync ───────────────────────────────────────────────

        [Theory]
        [InlineData(0, 1.0)]
        [InlineData(1, 1.0)]
        [InlineData(2, 1.2)]
        [InlineData(5, 1.2)]
        [InlineData(6, 1.5)]
        [InlineData(100, 1.5)]
        public async Task GetDemandMultiplierAsync_ReturnsCorrectMultiplier(int viewerCount, double expected)
        {
            // Arrange
            var slotId = Guid.NewGuid();
            _demandTrackingServiceMock.Setup(s => s.GetViewerCountAsync(slotId)).ReturnsAsync(viewerCount);

            // Act
            var result = await _service.GetDemandMultiplierAsync(slotId);

            // Assert
            result.Should().Be((decimal)expected);
        }

        // ─── GetTimeMultiplierAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task GetTimeMultiplierAsync_Returns1_WhenMoreThan24HoursAway()
        {
            var result = await _service.GetTimeMultiplierAsync(DateTime.UtcNow.AddHours(25));
            result.Should().Be(1.0m);
        }

        [Fact]
        public async Task GetTimeMultiplierAsync_Returns1_2_WhenBetween6And24Hours()
        {
            var result = await _service.GetTimeMultiplierAsync(DateTime.UtcNow.AddHours(12));
            result.Should().Be(1.2m);
        }

        [Fact]
        public async Task GetTimeMultiplierAsync_Returns1_5_WhenLessThan6Hours()
        {
            var result = await _service.GetTimeMultiplierAsync(DateTime.UtcNow.AddHours(2));
            result.Should().Be(1.5m);
        }

        [Fact]
        public async Task GetTimeMultiplierAsync_Returns1_2_WhenBetween6And24Hours_AtBoundary()
        {
            // Use 6.1 hours to safely be in the [6, 24] range (exactly 6 can slip to <6 due to execution time)
            var result = await _service.GetTimeMultiplierAsync(DateTime.UtcNow.AddHours(6.1));
            result.Should().Be(1.2m);
        }

        [Fact]
        public async Task GetTimeMultiplierAsync_Returns1_2_WhenExactly24Hours()
        {
            var result = await _service.GetTimeMultiplierAsync(DateTime.UtcNow.AddHours(24));
            result.Should().Be(1.2m);
        }

        // ─── GetDiscountFactorAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task GetDiscountFactorAsync_Returns1_WhenNoDiscounts()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            _discountRepositoryMock.Setup(r => r.GetByVenueIdAsync(venueId)).ReturnsAsync(new List<Discount>());

            // Act
            var result = await _service.GetDiscountFactorAsync(venueId, null, DateTime.UtcNow.AddHours(10));

            // Assert
            result.Should().Be(1.0m);
        }

        [Fact]
        public async Task GetDiscountFactorAsync_AppliesVenueDiscount_WhenActive()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var discount = new Discount
            {
                IsActive = true,
                PercentOff = 20,
                ValidFrom = now.AddDays(-1),
                ValidTo = now.AddDays(1)
            };
            _discountRepositoryMock.Setup(r => r.GetByVenueIdAsync(venueId)).ReturnsAsync(new List<Discount> { discount });

            // Act
            var result = await _service.GetDiscountFactorAsync(venueId, null, now.AddHours(5));

            // Assert
            result.Should().Be(0.8m); // 1 - 20/100
        }

        [Fact]
        public async Task GetDiscountFactorAsync_AppliesCourtDiscount_WhenActive()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var discount = new Discount
            {
                IsActive = true,
                PercentOff = 10,
                ValidFrom = now.AddDays(-1),
                ValidTo = now.AddDays(1)
            };
            _discountRepositoryMock.Setup(r => r.GetByCourtIdAsync(courtId)).ReturnsAsync(new List<Discount> { discount });

            // Act
            var result = await _service.GetDiscountFactorAsync(null, courtId, now.AddHours(5));

            // Assert
            result.Should().Be(0.9m); // 1 - 10/100
        }

        [Fact]
        public async Task GetDiscountFactorAsync_AppliesHigherDiscount_WhenBothVenueAndCourtDiscountsExist()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var venueDiscount = new Discount { IsActive = true, PercentOff = 10, ValidFrom = now.AddDays(-1), ValidTo = now.AddDays(1) };
            var courtDiscount = new Discount { IsActive = true, PercentOff = 25, ValidFrom = now.AddDays(-1), ValidTo = now.AddDays(1) };

            _discountRepositoryMock.Setup(r => r.GetByVenueIdAsync(venueId)).ReturnsAsync(new List<Discount> { venueDiscount });
            _discountRepositoryMock.Setup(r => r.GetByCourtIdAsync(courtId)).ReturnsAsync(new List<Discount> { courtDiscount });

            // Act
            var result = await _service.GetDiscountFactorAsync(venueId, courtId, now.AddHours(5));

            // Assert
            result.Should().Be(0.75m); // 1 - 25/100 (court discount is higher)
        }

        [Fact]
        public async Task GetDiscountFactorAsync_IgnoresInactiveDiscount()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var inactiveDiscount = new Discount
            {
                IsActive = false,
                PercentOff = 30,
                ValidFrom = now.AddDays(-1),
                ValidTo = now.AddDays(1)
            };
            _discountRepositoryMock.Setup(r => r.GetByVenueIdAsync(venueId)).ReturnsAsync(new List<Discount> { inactiveDiscount });

            // Act
            var result = await _service.GetDiscountFactorAsync(venueId, null, now.AddHours(5));

            // Assert
            result.Should().Be(1.0m); // No discount applied
        }

        [Fact]
        public async Task GetDiscountFactorAsync_IgnoresExpiredDiscount()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var expiredDiscount = new Discount
            {
                IsActive = true,
                PercentOff = 30,
                ValidFrom = now.AddDays(-10),
                ValidTo = now.AddDays(-1) // Expired yesterday
            };
            _discountRepositoryMock.Setup(r => r.GetByVenueIdAsync(venueId)).ReturnsAsync(new List<Discount> { expiredDiscount });

            // Act
            var result = await _service.GetDiscountFactorAsync(venueId, null, now.AddHours(5));

            // Assert
            result.Should().Be(1.0m); // No discount applied
        }

        // ─── GetHistoricalMultiplierAsync ───────────────────────────────────────────

        [Fact]
        public async Task GetHistoricalMultiplierAsync_Returns1_WhenNoHistoricalData()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            // No slots in the in-memory database

            // Act
            var result = await _service.GetHistoricalMultiplierAsync(courtId, DateTime.UtcNow.AddDays(1));

            // Assert
            result.Should().Be(1.0m);
        }

        [Fact]
        public async Task GetHistoricalMultiplierAsync_Returns1_WhenFewMatchingSlots()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var targetTime = now.AddDays(7); // 7 days in the future (same DayOfWeek)
            var dayOfWeek = targetTime.DayOfWeek;
            var hour = targetTime.Hour;

            // Seed 3 booked slots within the last 30 days on the same DayOfWeek and hour
            // (In a 30-day window, there are at most 4 occurrences of any given DayOfWeek)
            int count = 0;
            for (int daysBack = 1; daysBack <= 28 && count < 3; daysBack++)
            {
                var candidate = now.AddDays(-daysBack);
                if (candidate.DayOfWeek == dayOfWeek && candidate >= now.AddDays(-30))
                {
                    var slotTime = new DateTime(candidate.Year, candidate.Month, candidate.Day, hour, 0, 0, DateTimeKind.Utc);
                    _dbContext.Slots.Add(new Slot
                    {
                        Id = Guid.NewGuid(),
                        CourtId = courtId,
                        StartTime = slotTime,
                        Status = SlotStatus.Booked
                    });
                    count++;
                }
            }
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetHistoricalMultiplierAsync(courtId, targetTime);

            // Assert - with <= 10 matching slots, multiplier should be 1.0 (low demand)
            result.Should().Be(1.0m);
        }

        // ─── CalculatePriceAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task CalculatePriceAsync_ReturnsPriceCalculationDto()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var basePrice = 100m;
            var startTime = DateTime.UtcNow.AddHours(30); // > 24 hours: time multiplier = 1.0

            _demandTrackingServiceMock.Setup(s => s.GetViewerCountAsync(slotId)).ReturnsAsync(0); // demand = 1.0
            _discountRepositoryMock.Setup(r => r.GetByVenueIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Discount>());

            // Act
            var result = await _service.CalculatePriceAsync(slotId, basePrice, startTime);

            // Assert
            result.Should().NotBeNull();
            result.BasePrice.Should().Be(basePrice);
            result.DemandMultiplier.Should().Be(1.0m);
            result.TimeMultiplier.Should().Be(1.0m);
            result.FinalPrice.Should().Be(100m);
        }

        [Fact]
        public async Task CalculatePriceAsync_AppliesAllMultipliers()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var venueId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var basePrice = 100m;
            var startTime = DateTime.UtcNow.AddHours(12); // 6-24 hours: time multiplier = 1.2

            _demandTrackingServiceMock.Setup(s => s.GetViewerCountAsync(slotId)).ReturnsAsync(3); // demand = 1.2
            _discountRepositoryMock.Setup(r => r.GetByVenueIdAsync(venueId)).ReturnsAsync(new List<Discount>());
            _discountRepositoryMock.Setup(r => r.GetByCourtIdAsync(courtId)).ReturnsAsync(new List<Discount>());

            // Act
            var result = await _service.CalculatePriceAsync(slotId, basePrice, startTime, courtId, venueId);

            // Assert
            result.Should().NotBeNull();
            result.DemandMultiplier.Should().Be(1.2m);
            result.TimeMultiplier.Should().Be(1.2m);
            result.FinalPrice.Should().Be(Math.Round(100m * 1.2m * 1.2m * 1.0m * 1.0m, 2)); // 144.00
        }
    }
}
