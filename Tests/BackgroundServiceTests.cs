using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.BackgroundServices;
using Assignment_Example_HU.Services.Interfaces;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Data;

namespace Assignment_Example_HU.Tests.BackgroundServices
{
    public class BackgroundServiceTests
    {
        [Fact]
        public async Task SlotLockExpiryService_ExecutesCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SlotLockExpiryService>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var slotServiceMock = new Mock<ISlotService>();

            serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(ISlotService))).Returns(slotServiceMock.Object);

            var service = new SlotLockExpiryService(serviceProviderMock.Object, loggerMock.Object);
            var cts = new CancellationTokenSource();

            // Act
            var task = service.StartAsync(cts.Token);
            await Task.Delay(50);
            await cts.CancelAsync();
            await service.StopAsync(CancellationToken.None);

            // Assert
            slotServiceMock.Verify(x => x.ExpireLocksAsync(), Times.AtLeastOnce());
        }

        [Fact]
        public async Task RefundProcessingService_ExecutesCorrectly()
        {
            var loggerMock = new Mock<ILogger<RefundProcessingService>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var refundServiceMock = new Mock<IRefundService>();

            serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IRefundService))).Returns(refundServiceMock.Object);

            var service = new RefundProcessingService(serviceProviderMock.Object, loggerMock.Object);
            var cts = new CancellationTokenSource();

            var task = service.StartAsync(cts.Token);
            await Task.Delay(50);
            await cts.CancelAsync();
            await service.StopAsync(CancellationToken.None);

            refundServiceMock.Verify(x => x.ProcessPendingRefundsAsync(), Times.AtLeastOnce());
        }

        [Fact]
        public async Task GameAutoCancelService_ExecutesCorrectly()
        {
            var loggerMock = new Mock<ILogger<GameAutoCancelService>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var gameServiceMock = new Mock<IGameService>();

            serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IGameService))).Returns(gameServiceMock.Object);

            var service = new GameAutoCancelService(serviceProviderMock.Object, loggerMock.Object);
            var cts = new CancellationTokenSource();

            var task = service.StartAsync(cts.Token);
            await Task.Delay(50);
            await cts.CancelAsync();
            await service.StopAsync(CancellationToken.None);

            gameServiceMock.Verify(x => x.CancelGamesWithLowPlayersAsync(), Times.AtLeastOnce());
        }

        [Fact]
        public async Task WaitlistCleanupService_ExecutesCorrectly()
        {
            var loggerMock = new Mock<ILogger<WaitlistCleanupService>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var waitlistServiceMock = new Mock<IWaitlistService>();

            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var dbContext = new AppDbContext(options);

            serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IWaitlistService))).Returns(waitlistServiceMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(AppDbContext))).Returns(dbContext);

            var service = new WaitlistCleanupService(serviceProviderMock.Object, loggerMock.Object);
            var cts = new CancellationTokenSource();

            var task = service.StartAsync(cts.Token);
            await Task.Delay(50);
            await cts.CancelAsync();
            await service.StopAsync(CancellationToken.None);

            loggerMock.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.AtLeastOnce());
        }

        [Fact]
        public async Task DiscountExpiryService_ExecutesCorrectly()
        {
            var loggerMock = new Mock<ILogger<DiscountExpiryService>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var discountRepoMock = new Mock<IDiscountRepository>();
            var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var dbContext = new AppDbContext(options);

            serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IDiscountRepository))).Returns(discountRepoMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(AppDbContext))).Returns(dbContext);

            var service = new DiscountExpiryService(serviceProviderMock.Object, loggerMock.Object);
            var cts = new CancellationTokenSource();

            var task = service.StartAsync(cts.Token);
            await Task.Delay(50);
            await cts.CancelAsync();
            await service.StopAsync(CancellationToken.None);

            loggerMock.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.AtLeastOnce());
        }
    }
}
