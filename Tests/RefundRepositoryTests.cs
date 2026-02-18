using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories;

namespace Assignment_Example_HU.Tests.Repositories
{
    public class RefundRepositoryTests
    {
        private readonly AppDbContext _dbContext;
        private readonly RefundRepository _repository;

        public RefundRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);
            _repository = new RefundRepository(_dbContext);
        }

        [Fact]
        public async Task GetPendingRefundsAsync_ReturnsPendingOnly()
        {
            // Arrange
            var r1 = new Refund { Id = Guid.NewGuid(), Status = RefundStatus.Pending, Reason = "R1", TransactionId = Guid.NewGuid(), SlotId = Guid.NewGuid(), UserId = Guid.NewGuid() };
            var r2 = new Refund { Id = Guid.NewGuid(), Status = RefundStatus.Completed, Reason = "R2", TransactionId = Guid.NewGuid(), SlotId = Guid.NewGuid(), UserId = Guid.NewGuid() };
            _dbContext.Refunds.AddRange(r1, r2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetPendingRefundsAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(r1.Id);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsSortedRefunds()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var r1 = new Refund { Id = Guid.NewGuid(), UserId = userId, Status = RefundStatus.Pending, Reason = "R1", TransactionId = Guid.NewGuid(), SlotId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddHours(-1) };
            var r2 = new Refund { Id = Guid.NewGuid(), UserId = userId, Status = RefundStatus.Pending, Reason = "R2", TransactionId = Guid.NewGuid(), SlotId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
            _dbContext.Refunds.AddRange(r1, r2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = (await _repository.GetByUserIdAsync(userId)).ToList();

            // Assert
            result.Should().HaveCount(2);
            result[0].Reason.Should().Be("R2"); // Most recent first
        }
    }
}
