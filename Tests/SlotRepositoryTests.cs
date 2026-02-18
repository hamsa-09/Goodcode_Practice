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
    public class SlotRepositoryTests
    {
        private readonly AppDbContext _dbContext;
        private readonly SlotRepository _repository;

        public SlotRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);
            _repository = new SlotRepository(_dbContext);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsSlot_WhenExists()
        {
            // Arrange
            var slot = new Slot { Id = Guid.NewGuid(), Status = SlotStatus.Available };
            _dbContext.Slots.Add(slot);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(slot.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(slot.Id);
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_FiltersCorrectly()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var venueId = Guid.NewGuid();
            var venue = new Venue { Id = venueId, Name = "V1", Address = "A1", SportsSupported = "S1", ApprovalStatus = ApprovalStatus.Approved, CreatedAt = DateTime.UtcNow };
            var court = new Court { Id = courtId, VenueId = venueId, Name = "C1", SportType = SportType.Badminton, IsActive = true, OperatingHours = "09:00-22:00", BasePrice = 100 };
            _dbContext.Venues.Add(venue);
            _dbContext.Courts.Add(court);

            var now = DateTime.UtcNow;
            var s1 = new Slot { Id = Guid.NewGuid(), CourtId = courtId, StartTime = now.AddHours(1), Status = SlotStatus.Available };
            var s2 = new Slot { Id = Guid.NewGuid(), CourtId = courtId, StartTime = now.AddHours(2), Status = SlotStatus.Booked };
            var s3 = new Slot { Id = Guid.NewGuid(), CourtId = courtId, StartTime = now.AddDays(-1), Status = SlotStatus.Available }; // Past

            _dbContext.Slots.AddRange(s1, s2, s3);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetAvailableSlotsAsync(courtId: courtId);

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(s1.Id);
        }

        [Fact]
        public async Task GetExpiredLocksAsync_ReturnsExpiredOnly()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var s1 = new Slot { Id = Guid.NewGuid(), Status = SlotStatus.Locked, LockedUntil = now.AddMinutes(-1) }; // Expired
            var s2 = new Slot { Id = Guid.NewGuid(), Status = SlotStatus.Locked, LockedUntil = now.AddMinutes(1) };  // Not expired
            _dbContext.Slots.AddRange(s1, s2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetExpiredLocksAsync();

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(s1.Id);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesSlot()
        {
            // Arrange
            var slot = new Slot { Id = Guid.NewGuid(), Status = SlotStatus.Available };
            _dbContext.Slots.Add(slot);
            await _dbContext.SaveChangesAsync();

            // Act
            slot.Status = SlotStatus.Booked;
            await _repository.UpdateAsync(slot);
            await _repository.SaveChangesAsync();

            // Assert
            var updated = await _dbContext.Slots.FindAsync(slot.Id);
            updated!.Status.Should().Be(SlotStatus.Booked);
        }
    }
}
