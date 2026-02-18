using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Exceptions;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Tests.Services
{
    public class SlotServiceTests
    {
        private readonly Mock<ISlotRepository> _slotRepositoryMock;
        private readonly Mock<IPricingService> _pricingServiceMock;
        private readonly Mock<IDistributedLockService> _lockServiceMock;
        private readonly Mock<IDemandTrackingService> _demandTrackingServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SlotService _service;

        public SlotServiceTests()
        {
            _slotRepositoryMock = new Mock<ISlotRepository>();
            _pricingServiceMock = new Mock<IPricingService>();
            _lockServiceMock = new Mock<IDistributedLockService>();
            _demandTrackingServiceMock = new Mock<IDemandTrackingService>();
            _mapperMock = new Mock<IMapper>();

            _service = new SlotService(
                _slotRepositoryMock.Object,
                _pricingServiceMock.Object,
                _lockServiceMock.Object,
                _demandTrackingServiceMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_ReturnsMappedSlots()
        {
            // Arrange
            var slots = new List<Slot> { new Slot { Id = Guid.NewGuid(), Price = 100 } };
            _slotRepositoryMock.Setup(r => r.GetAvailableSlotsAsync(null, null, null, null))
                .ReturnsAsync(slots);

            _pricingServiceMock.Setup(s => s.CalculatePriceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
                .ReturnsAsync(new PriceCalculationDto { FinalPrice = 120 });

            _mapperMock.Setup(m => m.Map<AvailableSlotDto>(It.IsAny<Slot>()))
                .Returns(new AvailableSlotDto());

            // Act
            var result = await _service.GetAvailableSlotsAsync();

            // Assert
            result.Should().HaveCount(1);
            _demandTrackingServiceMock.Verify(s => s.IncrementViewerCountAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task LockSlotAsync_ThrowsConflict_WhenLockFails()
        {
            // Arrange
            _lockServiceMock.Setup(s => s.AcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = () => _service.LockSlotAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task LockSlotAsync_ThrowsNotFound_WhenSlotMissing()
        {
            // Arrange
            _lockServiceMock.Setup(s => s.AcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(true);
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Slot)null);

            // Act
            Func<Task> act = () => _service.LockSlotAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task LockSlotAsync_ReturnsResponse_WhenSuccessful()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Available };

            _lockServiceMock.Setup(s => s.AcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>())).ReturnsAsync(true);
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);
            _pricingServiceMock.Setup(s => s.CalculatePriceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
                .ReturnsAsync(new PriceCalculationDto { FinalPrice = 150 });
            _mapperMock.Setup(m => m.Map<BookSlotResponseDto>(slot)).Returns(new BookSlotResponseDto { SlotId = slotId });

            // Act
            var result = await _service.LockSlotAsync(slotId, userId);

            // Assert
            result.Should().NotBeNull();
            slot.Status.Should().Be(SlotStatus.Locked);
            slot.BookedByUserId.Should().Be(userId);
            _slotRepositoryMock.Verify(r => r.UpdateAsync(slot), Times.Once);
        }

        [Fact]
        public async Task ConfirmBookingAsync_Succeeds_WhenValid()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var lockKey = $"slot_{slotId}_user_{userId}";
            var slot = new Slot { Id = slotId, Status = SlotStatus.Locked, BookedByUserId = userId, LockedUntil = DateTime.UtcNow.AddMinutes(5) };

            _lockServiceMock.Setup(s => s.IsLockedAsync(lockKey)).ReturnsAsync(true);
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);
            _mapperMock.Setup(m => m.Map<BookSlotResponseDto>(slot)).Returns(new BookSlotResponseDto { Status = SlotStatus.Booked });

            // Act
            var result = await _service.ConfirmBookingAsync(slotId, userId);

            // Assert
            result.Status.Should().Be(SlotStatus.Booked);
            _slotRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Slot>()), Times.Once);
            _lockServiceMock.Verify(s => s.ReleaseLockAsync(lockKey), Times.Once);
        }

        [Fact]
        public async Task ReleaseLockAsync_ReturnsTrue_WhenValid()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var lockKey = $"slot_{slotId}_user_{userId}";
            var slot = new Slot { Id = slotId, Status = SlotStatus.Locked, BookedByUserId = userId };

            _lockServiceMock.Setup(s => s.IsLockedAsync(lockKey)).ReturnsAsync(true);
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);

            // Act
            var result = await _service.ReleaseLockAsync(slotId, userId);

            // Assert
            result.Should().BeTrue();
            slot.Status.Should().Be(SlotStatus.Available);
            _lockServiceMock.Verify(s => s.ReleaseLockAsync(lockKey), Times.Once);
        }

        [Fact]
        public async Task CancelBookingAsync_Succeeds_WhenUserIsOwner()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Booked, BookedByUserId = userId };
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);

            // Act
            var result = await _service.CancelBookingAsync(slotId, userId);

            // Assert
            result.Should().BeTrue();
            slot.Status.Should().Be(SlotStatus.Cancelled);
        }
    }
}
