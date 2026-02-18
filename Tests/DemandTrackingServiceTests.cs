using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Services;
using System.Text;
using System.Threading;

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
        public async Task GetViewerCountAsync_ReturnsZero_WhenCacheEmpty()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync((byte[])null);

            // Act
            var result = await _service.GetViewerCountAsync(slotId);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetViewerCountAsync_ReturnsValue_WhenCacheExists()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            var count = 5;
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(Encoding.UTF8.GetBytes(count.ToString()));

            // Act
            var result = await _service.GetViewerCountAsync(slotId);

            // Assert
            result.Should().Be(count);
        }

        [Fact]
        public async Task IncrementViewerCountAsync_IncrementsValue()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            // Start with 2
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(Encoding.UTF8.GetBytes("2"));

            // Act
            await _service.IncrementViewerCountAsync(slotId);

            // Assert
            // Should set to "3"
            _cacheMock.Verify(c => c.SetAsync(
                It.Is<string>(s => s.Contains(slotId.ToString())),
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "3"),
                It.IsAny<DistributedCacheEntryOptions>(),
                default), Times.Once);
        }

        [Fact]
        public async Task ResetViewerCountAsync_RemovesKey()
        {
            // Arrange
            var slotId = Guid.NewGuid();

            // Act
            await _service.ResetViewerCountAsync(slotId);

            // Assert
            _cacheMock.Verify(c => c.RemoveAsync(It.Is<string>(s => s.Contains(slotId.ToString())), default), Times.Once);
        }

        [Fact]
        public async Task GetViewerCountAsync_ReturnsZero_WhenCacheInvalid()
        {
            // Arrange
            var slotId = Guid.NewGuid();
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(Encoding.UTF8.GetBytes("invalid"));

            // Act
            var result = await _service.GetViewerCountAsync(slotId);

            // Assert
            result.Should().Be(0);
        }
    }
}
