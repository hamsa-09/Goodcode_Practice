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
        public async Task CreateDiscountAsync_Succeeds_ForCourt()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var dto = new CreateDiscountDto { Scope = DiscountScope.Court, CourtId = courtId, PercentOff = 15 };

            _courtServiceMock.Setup(s => s.IsCourtOwnerAsync(courtId, userId)).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<Discount>(dto)).Returns(new Discount());
            _mapperMock.Setup(m => m.Map<DiscountDto>(It.IsAny<Discount>())).Returns(new DiscountDto());

            // Act
            var result = await _service.CreateDiscountAsync(dto, userId);

            // Assert
            result.Should().NotBeNull();
            _discountRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Discount>()), Times.Once);
        }

        [Fact]
        public async Task CreateDiscountAsync_ThrowsUnauthorized_WhenNotVenueOwner()
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

        [Fact]
        public async Task CreateDiscountAsync_ThrowsUnauthorized_WhenNotCourtOwner()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var dto = new CreateDiscountDto { Scope = DiscountScope.Court, CourtId = courtId, PercentOff = 10 };

            _courtServiceMock.Setup(s => s.IsCourtOwnerAsync(courtId, userId)).ReturnsAsync(false);

            // Act
            Func<Task> act = () => _service.CreateDiscountAsync(dto, userId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task GetDiscountByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync((Discount)null);

            // Act
            var result = await _service.GetDiscountByIdAsync(discountId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDiscountByIdAsync_ReturnsMappedDiscount()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            var discount = new Discount { Id = discountId };
            _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync(discount);
            _mapperMock.Setup(m => m.Map<DiscountDto>(discount)).Returns(new DiscountDto { Id = discountId });

            // Act
            var result = await _service.GetDiscountByIdAsync(discountId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(discountId);
        }

        [Fact]
        public async Task GetDiscountsByVenueIdAsync_ReturnsMappedList()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var discounts = new List<Discount> { new Discount(), new Discount() };
            _discountRepositoryMock.Setup(r => r.GetByVenueIdAsync(venueId)).ReturnsAsync(discounts);
            _mapperMock.Setup(m => m.Map<IEnumerable<DiscountDto>>(discounts)).Returns(new List<DiscountDto> { new DiscountDto(), new DiscountDto() });

            // Act
            var result = await _service.GetDiscountsByVenueIdAsync(venueId);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetDiscountsByCourtIdAsync_ReturnsMappedList()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var discounts = new List<Discount> { new Discount() };
            _discountRepositoryMock.Setup(r => r.GetByCourtIdAsync(courtId)).ReturnsAsync(discounts);
            _mapperMock.Setup(m => m.Map<IEnumerable<DiscountDto>>(discounts)).Returns(new List<DiscountDto> { new DiscountDto() });

            // Act
            var result = await _service.GetDiscountsByCourtIdAsync(courtId);

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task UpdateDiscountAsync_ThrowsException_WhenNotFound()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync((Discount)null);

            // Act
            Func<Task> act = () => _service.UpdateDiscountAsync(discountId, new UpdateDiscountDto(), Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Discount not found.");
        }

        [Fact]
        public async Task UpdateDiscountAsync_ThrowsUnauthorized_WhenNotVenueOwner()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venueId = Guid.NewGuid();
            var discount = new Discount { Id = discountId, VenueId = venueId };

            _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync(discount);
            _venueServiceMock.Setup(s => s.IsVenueOwnerAsync(venueId, userId)).ReturnsAsync(false);

            // Act
            Func<Task> act = () => _service.UpdateDiscountAsync(discountId, new UpdateDiscountDto(), userId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task UpdateDiscountAsync_ThrowsUnauthorized_WhenNotCourtOwner()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var discount = new Discount { Id = discountId, CourtId = courtId };

            _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync(discount);
            _courtServiceMock.Setup(s => s.IsCourtOwnerAsync(courtId, userId)).ReturnsAsync(false);

            // Act
            Func<Task> act = () => _service.UpdateDiscountAsync(discountId, new UpdateDiscountDto(), userId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task UpdateDiscountAsync_Succeeds_WhenVenueOwner()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venueId = Guid.NewGuid();
            var discount = new Discount { Id = discountId, VenueId = venueId };

            _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync(discount);
            _venueServiceMock.Setup(s => s.IsVenueOwnerAsync(venueId, userId)).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<DiscountDto>(discount)).Returns(new DiscountDto { Id = discountId });

            // Act
            var result = await _service.UpdateDiscountAsync(discountId, new UpdateDiscountDto(), userId);

            // Assert
            result.Should().NotBeNull();
            _discountRepositoryMock.Verify(r => r.UpdateAsync(discount), Times.Once);
        }

        [Fact]
        public async Task DeleteDiscountAsync_ThrowsException_WhenNotFound()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync((Discount)null);

            // Act
            Func<Task> act = () => _service.DeleteDiscountAsync(discountId, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Discount not found.");
        }

        [Fact]
        public async Task DeleteDiscountAsync_ThrowsUnauthorized_WhenNotVenueOwner()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venueId = Guid.NewGuid();
            var discount = new Discount { Id = discountId, VenueId = venueId };

            _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync(discount);
            _venueServiceMock.Setup(s => s.IsVenueOwnerAsync(venueId, userId)).ReturnsAsync(false);

            // Act
            Func<Task> act = () => _service.DeleteDiscountAsync(discountId, userId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task DeleteDiscountAsync_Succeeds_WhenVenueOwner()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venueId = Guid.NewGuid();
            var discount = new Discount { Id = discountId, VenueId = venueId };

            _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync(discount);
            _venueServiceMock.Setup(s => s.IsVenueOwnerAsync(venueId, userId)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteDiscountAsync(discountId, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteDiscountAsync_Succeeds_WhenCourtOwner()
        {
            // Arrange
            var discountId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var courtId = Guid.NewGuid();
            var discount = new Discount { Id = discountId, CourtId = courtId };

            _discountRepositoryMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync(discount);
            _courtServiceMock.Setup(s => s.IsCourtOwnerAsync(courtId, userId)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteDiscountAsync(discountId, userId);

            // Assert
            result.Should().BeTrue();
        }
    }
}
