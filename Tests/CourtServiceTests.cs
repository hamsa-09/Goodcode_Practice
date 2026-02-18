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
using Assignment_Example_HU.Exceptions;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Repositories.Interfaces;
using Assignment_Example_HU.Services;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Tests.Services
{
    public class CourtServiceTests
    {
        private readonly Mock<ICourtRepository> _courtRepositoryMock;
        private readonly Mock<IVenueRepository> _venueRepositoryMock;
        private readonly Mock<IVenueService> _venueServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CourtService _service;

        public CourtServiceTests()
        {
            _courtRepositoryMock = new Mock<ICourtRepository>();
            _venueRepositoryMock = new Mock<IVenueRepository>();
            _venueServiceMock = new Mock<IVenueService>();
            _mapperMock = new Mock<IMapper>();

            _service = new CourtService(
                _courtRepositoryMock.Object,
                _venueRepositoryMock.Object,
                _venueServiceMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task CreateCourtAsync_Succeeds_WhenOwner()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var dto = new CreateCourtDto { VenueId = venueId, Name = "New Court" };

            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(new Venue());
            _venueServiceMock.Setup(s => s.IsVenueOwnerAsync(venueId, ownerId)).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<Court>(dto)).Returns(new Court { VenueId = venueId });
            _mapperMock.Setup(m => m.Map<CourtDto>(It.IsAny<Court>())).Returns(new CourtDto());
            _courtRepositoryMock.Setup(r => r.GetByIdWithVenueAsync(It.IsAny<Guid>())).ReturnsAsync(new Court());

            // Act
            var result = await _service.CreateCourtAsync(dto, ownerId);

            // Assert
            result.Should().NotBeNull();
            _courtRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Court>()), Times.Once);
        }

        [Fact]
        public async Task CreateCourtAsync_ThrowsNotFound_WhenVenueNotFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var dto = new CreateCourtDto { VenueId = venueId };
            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync((Venue)null);

            // Act
            Func<Task> act = () => _service.CreateCourtAsync(dto, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Venue not found.");
        }

        [Fact]
        public async Task CreateCourtAsync_ThrowsForbidden_WhenNotOwner()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var dto = new CreateCourtDto { VenueId = venueId, Name = "New Court" };

            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(new Venue());
            _venueServiceMock.Setup(s => s.IsVenueOwnerAsync(venueId, ownerId)).ReturnsAsync(false);

            // Act
            Func<Task> act = () => _service.CreateCourtAsync(dto, ownerId);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task GetCourtByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            _courtRepositoryMock.Setup(r => r.GetByIdWithVenueAsync(courtId)).ReturnsAsync((Court)null);

            // Act
            var result = await _service.GetCourtByIdAsync(courtId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCourtByIdAsync_ReturnsMappedCourt()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var court = new Court { Id = courtId };
            _courtRepositoryMock.Setup(r => r.GetByIdWithVenueAsync(courtId)).ReturnsAsync(court);
            _mapperMock.Setup(m => m.Map<CourtDto>(court)).Returns(new CourtDto { Id = courtId });

            // Act
            var result = await _service.GetCourtByIdAsync(courtId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(courtId);
        }

        [Fact]
        public async Task GetCourtsByVenueIdAsync_ReturnsMappedList()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var courts = new List<Court> { new Court(), new Court() };
            _courtRepositoryMock.Setup(r => r.GetByVenueIdAsync(venueId)).ReturnsAsync(courts);
            _mapperMock.Setup(m => m.Map<IEnumerable<CourtDto>>(courts)).Returns(new List<CourtDto> { new CourtDto(), new CourtDto() });

            // Act
            var result = await _service.GetCourtsByVenueIdAsync(venueId);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllCourtsAsync_ReturnsMappedList()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var courts = new List<Court> { new Court { Id = courtId } };
            var courtWithVenue = new Court { Id = courtId };

            _courtRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(courts);
            _courtRepositoryMock.Setup(r => r.GetByIdWithVenueAsync(courtId)).ReturnsAsync(courtWithVenue);
            _mapperMock.Setup(m => m.Map<CourtDto>(courtWithVenue)).Returns(new CourtDto { Id = courtId });

            // Act
            var result = await _service.GetAllCourtsAsync();

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task UpdateCourtAsync_ThrowsNotFound_WhenCourtNotFound()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            _courtRepositoryMock.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync((Court)null);

            // Act
            Func<Task> act = () => _service.UpdateCourtAsync(courtId, new UpdateCourtDto(), Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Court not found.");
        }

        [Fact]
        public async Task UpdateCourtAsync_ThrowsForbidden_WhenNotOwner()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var court = new Court { Id = courtId, VenueId = Guid.NewGuid() };

            _courtRepositoryMock.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync(court);
            _courtRepositoryMock.Setup(r => r.GetByIdWithVenueAsync(courtId)).ReturnsAsync(court);
            _venueServiceMock.Setup(s => s.IsVenueOwnerAsync(court.VenueId, userId)).ReturnsAsync(false);

            // Act
            Func<Task> act = () => _service.UpdateCourtAsync(courtId, new UpdateCourtDto(), userId);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task UpdateCourtAsync_Succeeds_WhenOwner()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venueId = Guid.NewGuid();
            var court = new Court { Id = courtId, VenueId = venueId };
            var courtWithVenue = new Court { Id = courtId, VenueId = venueId };

            _courtRepositoryMock.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync(court);
            _courtRepositoryMock.Setup(r => r.GetByIdWithVenueAsync(courtId)).ReturnsAsync(courtWithVenue);
            _venueServiceMock.Setup(s => s.IsVenueOwnerAsync(venueId, userId)).ReturnsAsync(true);
            _mapperMock.Setup(m => m.Map<CourtDto>(courtWithVenue)).Returns(new CourtDto { Id = courtId });

            // Act
            var result = await _service.UpdateCourtAsync(courtId, new UpdateCourtDto(), userId);

            // Assert
            result.Should().NotBeNull();
            _courtRepositoryMock.Verify(r => r.UpdateAsync(court), Times.Once);
        }

        [Fact]
        public async Task DeleteCourtAsync_ThrowsNotFound_WhenCourtNotFound()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            _courtRepositoryMock.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync((Court)null);

            // Act
            Func<Task> act = () => _service.DeleteCourtAsync(courtId, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DeleteCourtAsync_ThrowsForbidden_WhenNotOwner()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venueId = Guid.NewGuid();
            var court = new Court { Id = courtId, VenueId = venueId };

            _courtRepositoryMock.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync(court);
            _courtRepositoryMock.Setup(r => r.GetByIdWithVenueAsync(courtId)).ReturnsAsync(court);
            _venueServiceMock.Setup(s => s.IsVenueOwnerAsync(venueId, userId)).ReturnsAsync(false);

            // Act
            Func<Task> act = () => _service.DeleteCourtAsync(courtId, userId);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task DeleteCourtAsync_Succeeds_WhenOwner()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venueId = Guid.NewGuid();
            var court = new Court { Id = courtId, VenueId = venueId };

            _courtRepositoryMock.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync(court);
            _courtRepositoryMock.Setup(r => r.GetByIdWithVenueAsync(courtId)).ReturnsAsync(court);
            _venueServiceMock.Setup(s => s.IsVenueOwnerAsync(venueId, userId)).ReturnsAsync(true);

            // Act
            var result = await _service.DeleteCourtAsync(courtId, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsCourtOwnerAsync_ReturnsFalse_WhenCourtNotFound()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            _courtRepositoryMock.Setup(r => r.GetByIdWithVenueAsync(courtId)).ReturnsAsync((Court)null);

            // Act
            var result = await _service.IsCourtOwnerAsync(courtId, Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsCourtOwnerAsync_DelegatesToVenueService()
        {
            // Arrange
            var courtId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venueId = Guid.NewGuid();
            var court = new Court { Id = courtId, VenueId = venueId };

            _courtRepositoryMock.Setup(r => r.GetByIdWithVenueAsync(courtId)).ReturnsAsync(court);
            _venueServiceMock.Setup(s => s.IsVenueOwnerAsync(venueId, userId)).ReturnsAsync(true);

            // Act
            var result = await _service.IsCourtOwnerAsync(courtId, userId);

            // Assert
            result.Should().BeTrue();
        }
    }
}
