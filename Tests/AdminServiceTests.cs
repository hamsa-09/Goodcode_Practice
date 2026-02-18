using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Data;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Services;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Mappings;

namespace Assignment_Example_HU.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly IMapper _mapper;

        public AdminServiceTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task GetDashboardStatsAsync_ReturnsCorrectStats()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(new User { Id = Guid.NewGuid(), Email = "u1@test.com", UserName = "u1" });
                context.Venues.Add(new Venue { Id = Guid.NewGuid(), Name = "V1", Address = "A1", SportsSupported = "S1" });
                await context.SaveChangesAsync();
            }

            var repoMock = new Mock<ITransactionRepository>();
            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Transaction>
            {
                new Transaction { Id = Guid.NewGuid(), Amount = 100, Type = Enums.TransactionType.Debit, Status = Enums.TransactionStatus.Completed },
                new Transaction { Id = Guid.NewGuid(), Amount = 20, Type = Enums.TransactionType.Credit, Status = Enums.TransactionStatus.Completed, ReferenceId = "REFUND_1" }
            });

            using (var context = new AppDbContext(options))
            {
                var service = new AdminService(context, repoMock.Object, null!, _mapper);

                // Act
                var result = await service.GetDashboardStatsAsync();

                // Assert
                result.TotalUsers.Should().Be(1);
                result.TotalVenues.Should().Be(1);
                result.TotalRevenue.Should().Be(80);
            }
        }
    }
}
