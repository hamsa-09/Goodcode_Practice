using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
    public class VenueServiceTests
    {
        private readonly Mock<IVenueRepository> _venueRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly VenueService _service;

        public VenueServiceTests()
        {
            _venueRepositoryMock = new Mock<IVenueRepository>();
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _mapperMock = new Mock<IMapper>();

            _service = new VenueService(
                _venueRepositoryMock.Object,
                _userManagerMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task CreateVenueAsync_Succeeds_WhenOwner()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var dto = new CreateVenueDto { Name = "New Venue" };
            var owner = new User { Id = ownerId, Role = Role.VenueOwner };

            _userManagerMock.Setup(m => m.FindByIdAsync(ownerId.ToString())).ReturnsAsync(owner);
            _mapperMock.Setup(m => m.Map<Venue>(dto)).Returns(new Venue());
            _mapperMock.Setup(m => m.Map<VenueDto>(It.IsAny<Venue>())).Returns(new VenueDto());

            // Act
            var result = await _service.CreateVenueAsync(dto, ownerId);

            // Assert
            result.Should().NotBeNull();
            _venueRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Venue>()), Times.Once);
        }

        [Fact]
        public async Task CreateVenueAsync_ThrowsForbidden_WhenNotOwner()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var dto = new CreateVenueDto { Name = "New Venue" };
            var owner = new User { Id = ownerId, Role = Role.User };

            _userManagerMock.Setup(m => m.FindByIdAsync(ownerId.ToString())).ReturnsAsync(owner);

            // Act
            Func<Task> act = () => _service.CreateVenueAsync(dto, ownerId);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task GetVenueByIdAsync_ReturnsMappedVenue()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var venue = new Venue { Id = venueId };
            _venueRepositoryMock.Setup(r => r.GetByIdWithOwnerAsync(venueId)).ReturnsAsync(venue);
            _mapperMock.Setup(m => m.Map<VenueDto>(venue)).Returns(new VenueDto { Id = venueId });

            // Act
            var result = await _service.GetVenueByIdAsync(venueId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(venueId);
        }
    }
}
