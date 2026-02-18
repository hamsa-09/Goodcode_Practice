using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories;

namespace Assignment_Example_HU.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private readonly AppDbContext _dbContext;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new AppDbContext(options);
            _repository = new UserRepository(_dbContext);
        }

        [Fact]
        public async Task GetByEmailAsync_ReturnsUser_WhenExists()
        {
            // Arrange
            var email = "test@test.com";
            var user = new User { Id = Guid.NewGuid(), Email = email, UserName = "test", Role = Enums.Role.User };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByEmailAsync(email);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
        }

        [Fact]
        public async Task EmailExistsAsync_ReturnsTrue_WhenExists()
        {
            // Arrange
            var email = "exists@test.com";
            var user = new User { Id = Guid.NewGuid(), Email = email, UserName = "exists", Role = Enums.Role.User };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.EmailExistsAsync(email);

            // Assert
            result.Should().BeTrue();
        }
    }
}
