using System;
using System.Collections.Generic;
using System.Linq;
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
    public class RefundServiceTests
    {
        private readonly Mock<IRefundRepository> _refundRepositoryMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<ISlotRepository> _slotRepositoryMock;
        private readonly Mock<IWalletRepository> _walletRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AppDbContext _dbContext;
        private readonly RefundService _service;

        public RefundServiceTests()
        {
            _refundRepositoryMock = new Mock<IRefundRepository>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _slotRepositoryMock = new Mock<ISlotRepository>();
            _walletRepositoryMock = new Mock<IWalletRepository>();
            _mapperMock = new Mock<IMapper>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _dbContext = new AppDbContext(options);

            _service = new RefundService(
                _refundRepositoryMock.Object,
                _transactionRepositoryMock.Object,
                _slotRepositoryMock.Object,
                _walletRepositoryMock.Object,
                _dbContext,
                _mapperMock.Object);
        }

        [Theory]
        [InlineData(25, 100, 100)]
        [InlineData(10, 100, 50)]
        [InlineData(2, 100, 0)]
        public async Task CalculateRefundAmountAsync_ReturnsCorrectAmountBasedOnTime(double hoursUntil, decimal original, decimal expected)
        {
            // Arrange
            var startTime = DateTime.UtcNow.AddHours(hoursUntil);

            // Act
            var result = await _service.CalculateRefundAmountAsync(startTime, original);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public async Task RequestRefundAsync_ThrowsException_WhenSlotNotBooked()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var slotId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Available, BookedByUserId = userId };
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);

            // Act
            Func<Task> act = () => _service.RequestRefundAsync(userId, new RequestRefundDto { SlotId = slotId });

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Slot must be booked to request a refund.");
        }

        [Fact]
        public async Task RequestRefundAsync_Succeeds_WhenValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var slotId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Booked, BookedByUserId = userId, StartTime = DateTime.UtcNow.AddHours(25) };
            var transaction = new Transaction { Id = Guid.NewGuid(), RelatedSlotId = slotId, Type = TransactionType.Debit, Status = TransactionStatus.Completed, Amount = 100 };

            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);
            _transactionRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(new List<Transaction> { transaction });
            _refundRepositoryMock.Setup(r => r.GetByReferenceIdAsync(transaction.Id.ToString())).ReturnsAsync((Refund)null);

            var refundDto = new RefundDto { Id = Guid.NewGuid() };
            _mapperMock.Setup(m => m.Map<RefundDto>(It.IsAny<Refund>())).Returns(refundDto);

            // Act
            var result = await _service.RequestRefundAsync(userId, new RequestRefundDto { SlotId = slotId, Reason = "Change of plans" });

            // Assert
            result.Should().NotBeNull();
            _refundRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Refund>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPendingRefundsAsync_CompletesRefund_WhenValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var wallet = new Wallet { Id = walletId, UserId = userId, Balance = 500 };
            _dbContext.Wallets.Add(wallet);
            _dbContext.SaveChanges();

            var refund = new Refund
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TransactionId = Guid.NewGuid(),
                SlotId = Guid.NewGuid(),
                RefundAmount = 100,
                Status = RefundStatus.Pending
            };
            var transaction = new Transaction { Id = refund.TransactionId, Status = TransactionStatus.Completed };
            var slot = new Slot { Id = refund.SlotId, Status = SlotStatus.Booked };

            _refundRepositoryMock.Setup(r => r.GetPendingRefundsAsync()).ReturnsAsync(new List<Refund> { refund });
            _transactionRepositoryMock.Setup(r => r.GetByIdAsync(refund.TransactionId)).ReturnsAsync(transaction);
            _walletRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(wallet);
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(refund.SlotId)).ReturnsAsync(slot);

            // Act
            await _service.ProcessPendingRefundsAsync();

            // Assert
            refund.Status.Should().Be(RefundStatus.Completed);
            _walletRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Wallet>()), Times.Once);
        }
    }
}
