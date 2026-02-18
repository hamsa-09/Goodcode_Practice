using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Services;

namespace Assignment_Example_HU.Tests.Services
{
    public class DemandTrackingServiceTests
    {
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly DemandTrackingService _service;

        public DemandTrackingServiceTests()
        {
            _cacheMock = new Mock<IDistributedCache>();
            _service = new DemandTrackingService(_cacheMock.Object);
        }

        [Fact]
        public async Task IncrementViewerCountAsync_CallsSetString()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var key = $"slot_viewers:{slotId}";
            _cacheMock.Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>())).ReturnsAsync((byte[])null);

            // Act
            await _service.IncrementViewerCountAsync(slotId);

            // Assert
            _cacheMock.Verify(c => c.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetViewerCountAsync_ReturnsZero_WhenCacheEmpty()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var key = $"slot_viewers:{slotId}";
            _cacheMock.Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>())).ReturnsAsync((byte[])null);

            // Act
            var result = await _service.GetViewerCountAsync(slotId);

            // Assert
            result.Should().Be(0);
        }
    }
}
