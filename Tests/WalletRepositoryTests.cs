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
    public class WalletRepositoryTests
    {
        private readonly AppDbContext _dbContext;
        private readonly WalletRepository _repository;

        public WalletRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);
            _repository = new WalletRepository(_dbContext);
        }

        [Fact]
        public async Task GetByUserIdWithTransactionsAsync_ReturnsWalletAndTransactions()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 100 };
            var t1 = new Transaction { Id = Guid.NewGuid(), WalletId = wallet.Id, UserId = userId, Amount = 50, Type = Enums.TransactionType.Credit, Status = Enums.TransactionStatus.Completed };
            _dbContext.Wallets.Add(wallet);
            _dbContext.Transactions.Add(t1);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUserIdWithTransactionsAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.Transactions.Should().HaveCount(1);
        }

        [Fact]
        public async Task ExistsForUserAsync_ReturnsTrue_WhenExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 100 };
            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsForUserAsync(userId);

            // Assert
            result.Should().BeTrue();
        }
    }
}
