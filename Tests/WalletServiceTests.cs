using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
    public class WalletServiceTests
    {
        private readonly Mock<IWalletRepository> _walletRepositoryMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AppDbContext _dbContext;
        private readonly WalletService _service;

        public WalletServiceTests()
        {
            _walletRepositoryMock = new Mock<IWalletRepository>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _mapperMock = new Mock<IMapper>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _dbContext = new AppDbContext(options);

            _service = new WalletService(
                _walletRepositoryMock.Object,
                _transactionRepositoryMock.Object,
                _userManagerMock.Object,
                _dbContext,
                _mapperMock.Object);
        }

        [Fact]
        public async Task GetWalletAsync_CreatesNew_WhenNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _walletRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Wallet)null);
            _walletRepositoryMock.Setup(r => r.ExistsForUserAsync(userId)).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<WalletDto>(It.IsAny<Wallet>())).Returns(new WalletDto());

            // Act
            var result = await _service.GetWalletAsync(userId);

            // Assert
            result.Should().NotBeNull();
            _walletRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Wallet>()), Times.Once);
        }

        [Fact]
        public async Task AddFundsAsync_ThrowsException_WhenAmountZeroOrLess()
        {
            // Arrange
            var dto = new AddFundsDto { Amount = 0 };

            // Act
            Func<Task> act = () => _service.AddFundsAsync(Guid.NewGuid(), dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Amount must be greater than zero.");
        }

        [Fact]
        public async Task AddFundsAsync_Succeeds_WhenValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var wallet = new Wallet { Id = walletId, UserId = userId, Balance = 100 };

            _walletRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            var dto = new AddFundsDto { Amount = 50, Description = "Test" };
            _mapperMock.Setup(m => m.Map<TransactionDto>(It.IsAny<Transaction>())).Returns(new TransactionDto());

            // Act
            var result = await _service.AddFundsAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            var updatedWallet = await _dbContext.Wallets.FindAsync(walletId);
            updatedWallet.Balance.Should().Be(150);
        }
    }
}
