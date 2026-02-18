using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
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
        public async Task ProcessPaymentAsync_ThrowsException_WhenSlotNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var slotId = Guid.NewGuid();
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync((Slot)null);

            // Act
            Func<Task> act = () => _service.ProcessPaymentAsync(userId, new PaymentDto { SlotId = slotId });

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Slot not found.");
        }

        [Fact]
        public async Task ProcessPaymentAsync_ThrowsException_WhenSlotNotLockedByUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var slotId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Available, BookedByUserId = Guid.NewGuid() };
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);

            // Act
            Func<Task> act = () => _service.ProcessPaymentAsync(userId, new PaymentDto { SlotId = slotId });

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Slot must be locked by you before payment.");
        }

        [Fact]
        public async Task ProcessPaymentAsync_ThrowsException_WhenSlotLockedByDifferentUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var slotId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Locked, BookedByUserId = Guid.NewGuid() }; // Different user
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);

            // Act
            Func<Task> act = () => _service.ProcessPaymentAsync(userId, new PaymentDto { SlotId = slotId });

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Slot must be locked by you before payment.");
        }

        [Fact]
        public async Task ProcessPaymentAsync_ReturnsExisting_WhenIdempotentAndCompleted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var slotId = Guid.NewGuid();
            var referenceId = "ref-abc";
            var existingTx = new Transaction
            {
                Id = Guid.NewGuid(),
                Status = TransactionStatus.Completed,
                Amount = 100,
                RelatedSlotId = slotId,
                BalanceAfter = 400
            };
            _transactionRepositoryMock.Setup(r => r.GetByReferenceIdAsync(referenceId)).ReturnsAsync(existingTx);

            // Act
            var result = await _service.ProcessPaymentAsync(userId, new PaymentDto { SlotId = slotId, ReferenceId = referenceId });

            // Assert
            result.Should().NotBeNull();
            result.TransactionId.Should().Be(existingTx.Id);
            result.Amount.Should().Be(100);
            _slotRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPaymentAsync_CreatesWallet_WhenWalletNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var slotId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Locked, BookedByUserId = userId, Price = 50 };
            var wallet = new Wallet { Id = walletId, UserId = userId, Balance = 200 };

            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);
            _walletRepositoryMock.SetupSequence(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync((Wallet)null)   // First call: no wallet
                .ReturnsAsync(wallet);         // Second call: wallet after creation
            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            _mapperMock.Setup(m => m.Map<PaymentResponseDto>(It.IsAny<Transaction>())).Returns(new PaymentResponseDto());

            // Act
            var result = await _service.ProcessPaymentAsync(userId, new PaymentDto { SlotId = slotId });

            // Assert
            _walletServiceMock.Verify(s => s.CreateWalletAsync(userId), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ThrowsException_WhenInsufficientBalance()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var slotId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Locked, BookedByUserId = userId, Price = 500 };
            var wallet = new Wallet { Id = walletId, UserId = userId, Balance = 100 }; // Not enough

            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);
            _walletRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            // Act
            Func<Task> act = () => _service.ProcessPaymentAsync(userId, new PaymentDto { SlotId = slotId });

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Insufficient balance*");
        }

        [Fact]
        public async Task ProcessPaymentAsync_Succeeds_WhenSufficientBalance()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var slotId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Locked, BookedByUserId = userId, Price = 100 };
            var wallet = new Wallet { Id = walletId, UserId = userId, Balance = 500 };

            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);
            _walletRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            _mapperMock.Setup(m => m.Map<PaymentResponseDto>(It.IsAny<Transaction>())).Returns(new PaymentResponseDto { Amount = 100 });

            // Act
            var result = await _service.ProcessPaymentAsync(userId, new PaymentDto { SlotId = slotId });

            // Assert
            result.Should().NotBeNull();
            result.Amount.Should().Be(100);
            slot.Status.Should().Be(SlotStatus.Booked);
            _transactionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
            _walletRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Wallet>()), Times.Once);
            _slotRepositoryMock.Verify(r => r.UpdateAsync(slot), Times.Once);
        }
    }
}
