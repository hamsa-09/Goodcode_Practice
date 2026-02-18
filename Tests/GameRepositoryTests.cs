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
    public class GameRepositoryTests
    {
        private readonly AppDbContext _dbContext;
        private readonly GameRepository _repository;

        public GameRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);
            _repository = new GameRepository(_dbContext);
        }

        [Fact]
        public async Task GetByIdWithPlayersAsync_ReturnsCompleteObject()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), UserName = "U1", Email = "E1" };
            var game = new Game { Id = Guid.NewGuid(), CreatedByUserId = user.Id, Status = GameStatus.Pending };
            var gp = new GamePlayer { GameId = game.Id, UserId = user.Id, JoinedAt = DateTime.UtcNow };

            _dbContext.Users.Add(user);
            _dbContext.Games.Add(game);
            _dbContext.GamePlayers.Add(gp);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdWithPlayersAsync(game.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Players.Should().HaveCount(1);
            result.Players.First().User.UserName.Should().Be("U1");
        }

        [Fact]
        public async Task GetPendingGamesWithLowPlayersAsync_FiltersCorrectly()
        {
            // Arrange
            var game1 = new Game { Id = Guid.NewGuid(), Status = GameStatus.Pending, MinPlayers = 5 }; // 0 players < 5
            var game2 = new Game { Id = Guid.NewGuid(), Status = GameStatus.Pending, MinPlayers = 2 };
            var user1 = new User { Id = Guid.NewGuid(), UserName = "1", Email = "1" };
            var user2 = new User { Id = Guid.NewGuid(), UserName = "2", Email = "2" };
            var gp1 = new GamePlayer { GameId = game2.Id, UserId = user1.Id };
            var gp2 = new GamePlayer { GameId = game2.Id, UserId = user2.Id }; // 2 players == 2

            _dbContext.Games.AddRange(game1, game2);
            _dbContext.Users.AddRange(user1, user2);
            _dbContext.GamePlayers.AddRange(gp1, gp2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetPendingGamesWithLowPlayersAsync(0);

            // Assert
            result.Should().HaveCount(1);
            result.First().Id.Should().Be(game1.Id);
        }
    }
}
