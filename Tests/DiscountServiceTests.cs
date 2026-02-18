using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Enums;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Tests.Services
{
    public class DiscountServiceTests
    {
        private readonly Mock<IDiscountRepository> _discountRepositoryMock;
        private readonly Mock<IVenueRepository> _venueRepositoryMock;
        private readonly Mock<ICourtRepository> _courtRepositoryMock;
        private readonly Mock<IVenueService> _venueServiceMock;
        private readonly Mock<ICourtService> _courtServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly DiscountService _service;

        public DiscountServiceTests()
        {
            _discountRepositoryMock = new Mock<IDiscountRepository>();
            _venueRepositoryMock = new Mock<IVenueRepository>();
            _courtRepositoryMock = new Mock<ICourtRepository>();
            _venueServiceMock = new Mock<IVenueService>();
            _courtServiceMock = new Mock<ICourtService>();
            _mapperMock = new Mock<IMapper>();

            _service = new DiscountService(
                _discountRepositoryMock.Object,
                _venueRepositoryMock.Object,
                _courtRepositoryMock.Object,
                _venueServiceMock.Object,
                _courtServiceMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task CreateDiscountAsync_Succeeds_ForVenue()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var dto = new CreateDiscountDto { Scope = DiscountScope.Venue, VenueId = venueId, PercentOff = 10 };

            _venueServiceMock.Setup(s => s.IsVenueOwnerAsync(venueId, userId)).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<Discount>(dto)).Returns(new Discount());
            _mapperMock.Setup(m => m.Map<DiscountDto>(It.IsAny<Discount>())).Returns(new DiscountDto());

            // Act
            var result = await _service.CreateDiscountAsync(dto, userId);

            // Assert
            result.Should().NotBeNull();
            _discountRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Discount>()), Times.Once);
        }

        [Fact]
        public async Task CreateDiscountAsync_ThrowsUnauthorized_WhenNotOwner()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var dto = new CreateDiscountDto { Scope = DiscountScope.Venue, VenueId = venueId, PercentOff = 10 };

            _venueServiceMock.Setup(s => s.IsVenueOwnerAsync(venueId, userId)).ReturnsAsync(false);

            // Act
            Func<Task> act = () => _service.CreateDiscountAsync(dto, userId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
    }
}
