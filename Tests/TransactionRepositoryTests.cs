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
    public class TransactionRepositoryTests
    {
        private readonly AppDbContext _dbContext;
        private readonly TransactionRepository _repository;

        public TransactionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);
            _repository = new TransactionRepository(_dbContext);
        }

        [Fact]
        public async Task GetByReferenceIdAsync_ReturnsTransaction_WhenExists()
        {
            // Arrange
            var refId = "REF123";
            var t = new Transaction { Id = Guid.NewGuid(), ReferenceId = refId, WalletId = Guid.NewGuid(), UserId = Guid.NewGuid(), Amount = 10 };
            _dbContext.Transactions.Add(t);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByReferenceIdAsync(refId);

            // Assert
            result.Should().NotBeNull();
            result!.ReferenceId.Should().Be(refId);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsSortedTransactions()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var t1 = new Transaction { Id = Guid.NewGuid(), UserId = userId, WalletId = Guid.NewGuid(), Amount = 10, CreatedAt = DateTime.UtcNow.AddHours(-1) };
            var t2 = new Transaction { Id = Guid.NewGuid(), UserId = userId, WalletId = Guid.NewGuid(), Amount = 20, CreatedAt = DateTime.UtcNow };
            _dbContext.Transactions.AddRange(t1, t2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = (await _repository.GetByUserIdAsync(userId)).ToList();

            // Assert
            result.Should().HaveCount(2);
            result[0].Amount.Should().Be(20); // Most recent first
        }
    }
}
