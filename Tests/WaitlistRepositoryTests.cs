using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories;

namespace Assignment_Example_HU.Tests.Repositories
{
    public class WaitlistRepositoryTests
    {
        private readonly AppDbContext _dbContext;
        private readonly WaitlistRepository _repository;

        public WaitlistRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);
            _repository = new WaitlistRepository(_dbContext);
        }

        [Fact]
        public async Task GetByGameIdAsync_ReturnsSortedList()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var u1 = new User { Id = Guid.NewGuid(), UserName = "U1", Email = "E1" };
            var u2 = new User { Id = Guid.NewGuid(), UserName = "U2", Email = "E2" };
            var w1 = new Waitlist { Id = Guid.NewGuid(), GameId = gameId, UserId = u1.Id, Priority = 5, JoinedAt = DateTime.UtcNow.AddMinutes(-10) };
            var w2 = new Waitlist { Id = Guid.NewGuid(), GameId = gameId, UserId = u2.Id, Priority = 10, JoinedAt = DateTime.UtcNow.AddMinutes(-5) };

            _dbContext.Users.AddRange(u1, u2);
            _dbContext.Waitlists.AddRange(w1, w2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = (await _repository.GetByGameIdAsync(gameId)).ToList();

            // Assert
            result.Should().HaveCount(2);
            result[0].UserId.Should().Be(u2.Id); // Higher priority first
            result[1].UserId.Should().Be(u1.Id);
        }

        [Fact]
        public async Task RemoveAsync_RemovesItem()
        {
            // Arrange
            var item = new Waitlist { Id = Guid.NewGuid(), GameId = Guid.NewGuid(), UserId = Guid.NewGuid() };
            _dbContext.Waitlists.Add(item);
            await _dbContext.SaveChangesAsync();

            // Act
            await _repository.RemoveAsync(item);
            await _repository.SaveChangesAsync();

            // Assert
            _dbContext.Waitlists.Should().BeEmpty();
        }
    }
}
