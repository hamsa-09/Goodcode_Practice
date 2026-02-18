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
    public class CourtRepositoryTests
    {
        private readonly AppDbContext _dbContext;
        private readonly CourtRepository _repository;

        public CourtRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);
            _repository = new CourtRepository(_dbContext);
        }

        [Fact]
        public async Task GetByVenueIdAsync_ReturnsCourtsOfVenue()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var c1 = new Court { Id = Guid.NewGuid(), VenueId = venueId, Name = "C1", OperatingHours = "09:00-22:00" };
            var c2 = new Court { Id = Guid.NewGuid(), VenueId = Guid.NewGuid(), Name = "C2", OperatingHours = "09:00-22:00" };
            _dbContext.Courts.AddRange(c1, c2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByVenueIdAsync(venueId);

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(c1.Id);
        }

        [Fact]
        public async Task GetByIdWithVenueAsync_ReturnsCourtAndVenue()
        {
            // Arrange
            var venue = new Venue { Id = Guid.NewGuid(), Name = "V1", Address = "A1", SportsSupported = "S1", OwnerId = Guid.NewGuid() };
            var court = new Court { Id = Guid.NewGuid(), VenueId = venue.Id, Name = "C1", OperatingHours = "09:00-22:00" };
            _dbContext.Venues.Add(venue);
            _dbContext.Courts.Add(court);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdWithVenueAsync(court.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Venue.Should().NotBeNull();
            result.Venue.Name.Should().Be("V1");
        }
    }
}
