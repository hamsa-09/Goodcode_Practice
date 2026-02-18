using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories;

namespace Assignment_Example_HU.Tests.Repositories
{
    public class DiscountRepositoryTests
    {
        private readonly AppDbContext _dbContext;
        private readonly DiscountRepository _repository;

        public DiscountRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);
            _repository = new DiscountRepository(_dbContext);
        }

        [Fact]
        public async Task GetActiveDiscountsAsync_ReturnsCurrentOnly()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var d1 = new Discount { Id = Guid.NewGuid(), IsActive = true, ValidFrom = now.AddHours(-1), ValidTo = now.AddHours(1), PercentOff = 10, Scope = Enums.DiscountScope.Venue };
            var d2 = new Discount { Id = Guid.NewGuid(), IsActive = true, ValidFrom = now.AddDays(-2), ValidTo = now.AddDays(-1), PercentOff = 20, Scope = Enums.DiscountScope.Venue }; // Expired
            _dbContext.Discounts.AddRange(d1, d2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveDiscountsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(d1.Id);
        }

        [Fact]
        public async Task GetByCourtIdAsync_ReturnsCourtsDiscounts()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var d = new Discount { Id = Guid.NewGuid(), CourtId = courtId, PercentOff = 15, IsActive = true, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(1), Scope = Enums.DiscountScope.Court };
            _dbContext.Discounts.Add(d);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByCourtIdAsync(courtId);

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(d.Id);
        }
    }
}
