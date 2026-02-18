using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Services;

namespace Assignment_Example_HU.Tests.Services
{
    public class DistributedLockServiceTests
    {
        private readonly IMemoryCache _cache;
        private readonly DistributedLockService _service;

        public DistributedLockServiceTests()
        {
            // Use real MemoryCache because mocking TryGetValue with out params is painful and unnecessary
            _cache = new MemoryCache(new MemoryCacheOptions());
            _service = new DistributedLockService(_cache);
        }

        [Fact]
        public async Task AcquireLockAsync_ReturnsTrue_WhenNotLocked()
        {
            // Arrange
            var key = "test_lock";

            // Act
            var result = await _service.AcquireLockAsync(key, TimeSpan.FromMinutes(1));

            // Assert
            result.Should().BeTrue();
            (await _service.IsLockedAsync(key)).Should().BeTrue();
        }

        [Fact]
        public async Task AcquireLockAsync_ReturnsFalse_WhenAlreadyLocked()
        {
            // Arrange
            var key = "test_lock";
            await _service.AcquireLockAsync(key, TimeSpan.FromMinutes(1));

            // Act
            var result = await _service.AcquireLockAsync(key, TimeSpan.FromMinutes(1));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ReleaseLockAsync_AllowsReAcquiring()
        {
            // Arrange
            var key = "test_lock";
            await _service.AcquireLockAsync(key, TimeSpan.FromMinutes(1));
            (await _service.IsLockedAsync(key)).Should().BeTrue();

            // Act
            await _service.ReleaseLockAsync(key);

            // Assert
            (await _service.IsLockedAsync(key)).Should().BeFalse();
            (await _service.AcquireLockAsync(key, TimeSpan.FromMinutes(1))).Should().BeTrue();
        }

        [Fact]
        public async Task IsLockedAsync_ReturnsCorrectStatus()
        {
            // Arrange
            var key = "test_lock";

            // Act & Assert
            (await _service.IsLockedAsync(key)).Should().BeFalse();
            await _service.AcquireLockAsync(key, TimeSpan.FromMinutes(1));
            (await _service.IsLockedAsync(key)).Should().BeTrue();
        }
    }
}
