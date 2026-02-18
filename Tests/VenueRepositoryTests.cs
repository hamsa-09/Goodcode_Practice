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
    public class VenueRepositoryTests
    {
        private readonly AppDbContext _dbContext;
        private readonly VenueRepository _repository;

        public VenueRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);
            _repository = new VenueRepository(_dbContext);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllVenues()
        {
            // Arrange
            var v1 = new Venue { Id = Guid.NewGuid(), Name = "A1", Address = "Ad1", SportsSupported = "S1", ApprovalStatus = ApprovalStatus.Approved, OwnerId = Guid.NewGuid() };
            var v2 = new Venue { Id = Guid.NewGuid(), Name = "A2", Address = "Ad2", SportsSupported = "S2", ApprovalStatus = ApprovalStatus.Pending, OwnerId = Guid.NewGuid() };
            _dbContext.Venues.AddRange(v1, v2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByOwnerIdAsync_ReturnsAllOwnerVenues()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var v1 = new Venue { Id = Guid.NewGuid(), Name = "A1", OwnerId = ownerId, Address = "Ad1", SportsSupported = "S1" };
            var v2 = new Venue { Id = Guid.NewGuid(), Name = "A2", OwnerId = Guid.NewGuid(), Address = "Ad2", SportsSupported = "S2" };
            _dbContext.Venues.AddRange(v1, v2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByOwnerIdAsync(ownerId);

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(v1.Id);
        }
    }
}
