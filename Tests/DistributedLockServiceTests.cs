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
            _cache = new MemoryCache(new MemoryCacheOptions());
            _service = new DistributedLockService(_cache);
        }

        [Fact]
        public async Task AcquireLockAsync_ReturnsTrue_WhenNotLocked()
        {
            // Act
            var result = await _service.AcquireLockAsync("test_lock", TimeSpan.FromMinutes(1));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AcquireLockAsync_ReturnsFalse_WhenAlreadyLocked()
        {
            // Arrange
            await _service.AcquireLockAsync("test_lock", TimeSpan.FromMinutes(1));

            // Act
            var result = await _service.AcquireLockAsync("test_lock", TimeSpan.FromMinutes(1));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ReleaseLockAsync_Works()
        {
            // Arrange
            await _service.AcquireLockAsync("test_lock", TimeSpan.FromMinutes(1));

            // Act
            await _service.ReleaseLockAsync("test_lock");
            var isLocked = await _service.IsLockedAsync("test_lock");

            // Assert
            isLocked.Should().BeFalse();
        }
    }
}
