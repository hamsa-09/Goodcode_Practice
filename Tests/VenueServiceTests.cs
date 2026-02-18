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
        public async Task CreateVenueAsync_Succeeds_WhenAdmin()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var dto = new CreateVenueDto { Name = "New Venue" };
            var owner = new User { Id = ownerId, Role = Role.Admin };

            _userManagerMock.Setup(m => m.FindByIdAsync(ownerId.ToString())).ReturnsAsync(owner);
            _mapperMock.Setup(m => m.Map<Venue>(dto)).Returns(new Venue());
            _mapperMock.Setup(m => m.Map<VenueDto>(It.IsAny<Venue>())).Returns(new VenueDto());

            // Act
            var result = await _service.CreateVenueAsync(dto, ownerId);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateVenueAsync_ThrowsNotFound_WhenOwnerNotFound()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            _userManagerMock.Setup(m => m.FindByIdAsync(ownerId.ToString())).ReturnsAsync((User)null);

            // Act
            Func<Task> act = () => _service.CreateVenueAsync(new CreateVenueDto(), ownerId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Owner not found.");
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
        public async Task GetVenueByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            _venueRepositoryMock.Setup(r => r.GetByIdWithOwnerAsync(venueId)).ReturnsAsync((Venue)null);

            // Act
            var result = await _service.GetVenueByIdAsync(venueId);

            // Assert
            result.Should().BeNull();
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

        [Fact]
        public async Task GetVenuesByOwnerAsync_ReturnsMappedList()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var venues = new List<Venue> { new Venue(), new Venue() };
            _venueRepositoryMock.Setup(r => r.GetByOwnerIdAsync(ownerId)).ReturnsAsync(venues);
            _mapperMock.Setup(m => m.Map<IEnumerable<VenueDto>>(venues)).Returns(new List<VenueDto> { new VenueDto(), new VenueDto() });

            // Act
            var result = await _service.GetVenuesByOwnerAsync(ownerId);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllVenuesAsync_ReturnsMappedList()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var venues = new List<Venue> { new Venue { Id = venueId } };
            var venueWithOwner = new Venue { Id = venueId };

            _venueRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(venues);
            _venueRepositoryMock.Setup(r => r.GetByIdWithOwnerAsync(venueId)).ReturnsAsync(venueWithOwner);
            _mapperMock.Setup(m => m.Map<VenueDto>(venueWithOwner)).Returns(new VenueDto { Id = venueId });

            // Act
            var result = await _service.GetAllVenuesAsync();

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task UpdateVenueAsync_ThrowsNotFound_WhenVenueNotFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync((Venue)null);

            // Act
            Func<Task> act = () => _service.UpdateVenueAsync(venueId, new UpdateVenueDto(), Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Venue not found.");
        }

        [Fact]
        public async Task UpdateVenueAsync_ThrowsForbidden_WhenNotOwner()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venue = new Venue { Id = venueId, OwnerId = Guid.NewGuid() };

            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(venue);
            // IsVenueOwnerAsync will call GetByIdAsync and FindByIdAsync
            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(venue);
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(new User { Role = Role.User });

            // Act
            Func<Task> act = () => _service.UpdateVenueAsync(venueId, new UpdateVenueDto(), userId);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task UpdateVenueAsync_Succeeds_WhenOwner()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venue = new Venue { Id = venueId, OwnerId = userId };
            var venueWithOwner = new Venue { Id = venueId, OwnerId = userId };

            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(venue);
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(new User { Role = Role.VenueOwner });
            _venueRepositoryMock.Setup(r => r.GetByIdWithOwnerAsync(venueId)).ReturnsAsync(venueWithOwner);
            _mapperMock.Setup(m => m.Map<VenueDto>(venueWithOwner)).Returns(new VenueDto { Id = venueId });

            // Act
            var result = await _service.UpdateVenueAsync(venueId, new UpdateVenueDto(), userId);

            // Assert
            result.Should().NotBeNull();
            _venueRepositoryMock.Verify(r => r.UpdateAsync(venue), Times.Once);
        }

        [Fact]
        public async Task UpdateVenueApprovalAsync_ThrowsNotFound_WhenVenueNotFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync((Venue)null);

            // Act
            Func<Task> act = () => _service.UpdateVenueApprovalAsync(venueId, new UpdateVenueApprovalDto());

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateVenueApprovalAsync_Succeeds()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var venue = new Venue { Id = venueId, ApprovalStatus = ApprovalStatus.Pending };
            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(venue);

            // Act
            var result = await _service.UpdateVenueApprovalAsync(venueId, new UpdateVenueApprovalDto { ApprovalStatus = ApprovalStatus.Approved });

            // Assert
            result.Should().BeTrue();
            venue.ApprovalStatus.Should().Be(ApprovalStatus.Approved);
        }

        [Fact]
        public async Task DeleteVenueAsync_ThrowsNotFound_WhenVenueNotFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync((Venue)null);

            // Act
            Func<Task> act = () => _service.DeleteVenueAsync(venueId, Guid.NewGuid());

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DeleteVenueAsync_ThrowsForbidden_WhenNotOwner()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venue = new Venue { Id = venueId, OwnerId = Guid.NewGuid() };

            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(venue);
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(new User { Role = Role.User });

            // Act
            Func<Task> act = () => _service.DeleteVenueAsync(venueId, userId);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task DeleteVenueAsync_Succeeds_WhenOwner()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venue = new Venue { Id = venueId, OwnerId = userId };

            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(venue);
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(new User { Role = Role.VenueOwner });

            // Act
            var result = await _service.DeleteVenueAsync(venueId, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsVenueOwnerAsync_ReturnsTrue_WhenOwner()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venue = new Venue { Id = venueId, OwnerId = userId };

            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(venue);
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(new User { Role = Role.VenueOwner });

            // Act
            var result = await _service.IsVenueOwnerAsync(venueId, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsVenueOwnerAsync_ReturnsTrue_WhenAdmin()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venue = new Venue { Id = venueId, OwnerId = Guid.NewGuid() }; // Different owner

            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(venue);
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync(new User { Role = Role.Admin });

            // Act
            var result = await _service.IsVenueOwnerAsync(venueId, userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsVenueOwnerAsync_ReturnsFalse_WhenVenueNotFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync((Venue)null);

            // Act
            var result = await _service.IsVenueOwnerAsync(venueId, Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsVenueOwnerAsync_ReturnsFalse_WhenUserNotFound()
        {
            // Arrange
            var venueId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var venue = new Venue { Id = venueId, OwnerId = Guid.NewGuid() };

            _venueRepositoryMock.Setup(r => r.GetByIdAsync(venueId)).ReturnsAsync(venue);
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString())).ReturnsAsync((User)null);

            // Act
            var result = await _service.IsVenueOwnerAsync(venueId, userId);

            // Assert
            result.Should().BeFalse();
        }
    }
}
