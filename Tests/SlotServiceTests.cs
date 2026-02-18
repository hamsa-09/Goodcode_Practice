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
        public async Task GetAvailableSlotsAsync_WithFilters_PassesFiltersToRepository()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var venueId = Guid.NewGuid();
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(7);

            _slotRepositoryMock.Setup(r => r.GetAvailableSlotsAsync(courtId, venueId, startDate, endDate))
                .ReturnsAsync(new List<Slot>());

            // Act
            var result = await _service.GetAvailableSlotsAsync(courtId, venueId, startDate, endDate);

            // Assert
            result.Should().BeEmpty();
            _slotRepositoryMock.Verify(r => r.GetAvailableSlotsAsync(courtId, venueId, startDate, endDate), Times.Once);
        }

        [Fact]
        public async Task GetSlotDetailsAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            _slotRepositoryMock.Setup(r => r.GetByIdWithCourtAsync(slotId)).ReturnsAsync((Slot)null);

            // Act
            var result = await _service.GetSlotDetailsAsync(slotId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSlotDetailsAsync_ReturnsMappedSlot_WhenFound()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Price = 100 };
            _slotRepositoryMock.Setup(r => r.GetByIdWithCourtAsync(slotId)).ReturnsAsync(slot);
            _pricingServiceMock.Setup(s => s.CalculatePriceAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<Guid?>()))
                .ReturnsAsync(new PriceCalculationDto { FinalPrice = 120 });
            _mapperMock.Setup(m => m.Map<AvailableSlotDto>(slot)).Returns(new AvailableSlotDto { Id = slotId });

            // Act
            var result = await _service.GetSlotDetailsAsync(slotId);

            // Assert
            result.Should().NotBeNull();
            _demandTrackingServiceMock.Verify(s => s.IncrementViewerCountAsync(slotId), Times.Once);
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
        public async Task LockSlotAsync_ThrowsBadRequest_WhenSlotNotAvailable()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Booked };

            _lockServiceMock.Setup(s => s.AcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>())).ReturnsAsync(true);
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);

            // Act
            Func<Task> act = () => _service.LockSlotAsync(slotId, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
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
        public async Task ConfirmBookingAsync_ThrowsBadRequest_WhenNoLock()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var lockKey = $"slot_{slotId}_user_{userId}";
            _lockServiceMock.Setup(s => s.IsLockedAsync(lockKey)).ReturnsAsync(false);

            // Act
            Func<Task> act = () => _service.ConfirmBookingAsync(slotId, userId);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("You must lock the slot first before confirming.");
        }

        [Fact]
        public async Task ConfirmBookingAsync_ThrowsNotFound_WhenSlotMissing()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var lockKey = $"slot_{slotId}_user_{userId}";

            _lockServiceMock.Setup(s => s.IsLockedAsync(lockKey)).ReturnsAsync(true);
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync((Slot)null);

            // Act
            Func<Task> act = () => _service.ConfirmBookingAsync(slotId, userId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task ConfirmBookingAsync_ThrowsForbidden_WhenSlotNotLockedByUser()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var lockKey = $"slot_{slotId}_user_{userId}";
            var slot = new Slot { Id = slotId, Status = SlotStatus.Booked, BookedByUserId = Guid.NewGuid() };

            _lockServiceMock.Setup(s => s.IsLockedAsync(lockKey)).ReturnsAsync(true);
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);

            // Act
            Func<Task> act = () => _service.ConfirmBookingAsync(slotId, userId);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task ConfirmBookingAsync_ThrowsBadRequest_WhenLockExpired()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var lockKey = $"slot_{slotId}_user_{userId}";
            var slot = new Slot
            {
                Id = slotId,
                Status = SlotStatus.Locked,
                BookedByUserId = userId,
                LockedUntil = DateTime.UtcNow.AddMinutes(-5) // Expired
            };

            _lockServiceMock.Setup(s => s.IsLockedAsync(lockKey)).ReturnsAsync(true);
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);

            // Act
            Func<Task> act = () => _service.ConfirmBookingAsync(slotId, userId);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Price lock has expired. Please lock the slot again.");
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
        public async Task ReleaseLockAsync_ReturnsFalse_WhenNoLock()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var lockKey = $"slot_{slotId}_user_{userId}";
            _lockServiceMock.Setup(s => s.IsLockedAsync(lockKey)).ReturnsAsync(false);

            // Act
            var result = await _service.ReleaseLockAsync(slotId, userId);

            // Assert
            result.Should().BeFalse();
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
        public async Task CancelBookingAsync_ThrowsNotFound_WhenSlotMissing()
        {
            // Arrange
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Slot)null);

            // Act
            Func<Task> act = () => _service.CancelBookingAsync(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CancelBookingAsync_ThrowsUnauthorized_WhenNotOwner()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Booked, BookedByUserId = Guid.NewGuid() };
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);

            // Act
            Func<Task> act = () => _service.CancelBookingAsync(slotId, userId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task CancelBookingAsync_ThrowsBadRequest_WhenNotBooked()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var slot = new Slot { Id = slotId, Status = SlotStatus.Available, BookedByUserId = userId };
            _slotRepositoryMock.Setup(r => r.GetByIdAsync(slotId)).ReturnsAsync(slot);

            // Act
            Func<Task> act = () => _service.CancelBookingAsync(slotId, userId);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Slot is not booked.");
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

        [Fact]
        public async Task ExpireLocksAsync_ReleasesExpiredLocks()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var expiredSlot = new Slot { Id = slotId, Status = SlotStatus.Locked, BookedByUserId = userId };

            _slotRepositoryMock.Setup(r => r.GetExpiredLocksAsync()).ReturnsAsync(new List<Slot> { expiredSlot });

            // Act
            await _service.ExpireLocksAsync();

            // Assert
            expiredSlot.Status.Should().Be(SlotStatus.Available);
            expiredSlot.LockedUntil.Should().BeNull();
            _slotRepositoryMock.Verify(r => r.UpdateAsync(expiredSlot), Times.Once);
            _slotRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
