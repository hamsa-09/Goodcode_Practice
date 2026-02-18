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
    }
}
