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
    public class RatingRepositoryTests
    {
        private readonly AppDbContext _dbContext;
        private readonly RatingRepository _repository;

        public RatingRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);
            _repository = new RatingRepository(_dbContext);
        }

        [Fact]
        public async Task GetByVenueIdAsync_ReturnsSortedRatings()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var user = new User { Id = Guid.NewGuid(), UserName = "U1", Email = "E1" };
            var r1 = new Rating { Id = Guid.NewGuid(), VenueId = venueId, RatedById = user.Id, Score = 4, CreatedAt = DateTime.UtcNow.AddDays(-1), Type = RatingType.Venue };
            var r2 = new Rating { Id = Guid.NewGuid(), VenueId = venueId, RatedById = user.Id, Score = 5, CreatedAt = DateTime.UtcNow, Type = RatingType.Venue };

            _dbContext.Users.Add(user);
            _dbContext.Ratings.AddRange(r1, r2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = (await _repository.GetByVenueIdAsync(venueId)).ToList();

            // Assert
            result.Should().HaveCount(2);
            result[0].Score.Should().Be(5); // Most recent first
        }

        [Fact]
        public async Task HasRatedAsync_ReturnsTrue_WhenExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var r = new Rating { Id = Guid.NewGuid(), RatedById = userId, GameId = gameId, Type = RatingType.Player, Score = 5 };
            _dbContext.Ratings.Add(r);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.HasRatedAsync(userId, gameId, RatingType.Player);

            // Assert
            result.Should().BeTrue();
        }
    }
}
