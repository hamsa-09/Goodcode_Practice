using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IWalletService> _walletServiceMock;
        private readonly Mock<ISlotRepository> _slotRepositoryMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IWalletRepository> _walletRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AppDbContext _dbContext;
        private readonly PaymentService _service;

        public PaymentServiceTests()
        {
            _walletServiceMock = new Mock<IWalletService>();
            _slotRepositoryMock = new Mock<ISlotRepository>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _walletRepositoryMock = new Mock<IWalletRepository>();
            _mapperMock = new Mock<IMapper>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _dbContext = new AppDbContext(options);

            _service = new PaymentService(
                _walletServiceMock.Object,
                _slotRepositoryMock.Object,
                _transactionRepositoryMock.Object,
                _walletRepositoryMock.Object,
                _dbContext,
                _mapperMock.Object);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ReturnsExistingTransaction_WhenAlreadyCompleted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new PaymentDto { ReferenceId = "ref123", SlotId = Guid.NewGuid() };
            var existingTx = new Transaction { Id = Guid.NewGuid(), Status = TransactionStatus.Completed, Amount = 100 };
            _transactionRepositoryMock.Setup(r => r.GetByReferenceIdAsync("ref123")).ReturnsAsync(existingTx);

            // Act
            var result = await _service.ProcessPaymentAsync(userId, dto);

            // Assert
            result.TransactionId.Should().Be(existingTx.Id);
            _slotRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ThrowsException_WhenSlotNotFound()
        {
            // Arrange
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Slot)null);
            var dto = new PaymentDto { SlotId = Guid.NewGuid() };

            // Act
            Func<Task> act = () => _service.ProcessPaymentAsync(Guid.NewGuid(), dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Slot not found.");
        }

        [Fact]
        public async Task ProcessPaymentAsync_ThrowsException_WhenInsufficientBalance()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var slotId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Locked, BookedByUserId = userId, Price = 500 };
            var wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Balance = 100 };

            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);
            _walletRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);

            // Seed In-Memory DB for the wallet lock check
            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            var dto = new PaymentDto { SlotId = slotId };

            // Act
            Func<Task> act = () => _service.ProcessPaymentAsync(userId, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Insufficient balance*");
        }

        [Fact]
        public async Task ProcessPaymentAsync_Succeeds_WhenBalanceIsCorrect()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var slotId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Locked, BookedByUserId = userId, Price = 100 };
            var wallet = new Wallet { Id = walletId, UserId = userId, Balance = 200 };

            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);
            _walletRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);

            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            var dto = new PaymentDto { SlotId = slotId };
            _mapperMock.Setup(m => m.Map<PaymentResponseDto>(It.IsAny<Transaction>()))
                .Returns(new PaymentResponseDto { Amount = 100 });

            // Act
            var result = await _service.ProcessPaymentAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            slot.Status.Should().Be(SlotStatus.Booked);
            var updatedWallet = await _dbContext.Wallets.FindAsync(walletId);
            updatedWallet.Balance.Should().Be(100);
        }
    }
}
