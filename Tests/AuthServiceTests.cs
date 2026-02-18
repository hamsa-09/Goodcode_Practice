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
using Assignment_Example_HU.Services;
using Assignment_Example_HU.Services.Interfaces;
using System.Threading;

namespace Assignment_Example_HU.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            _tokenServiceMock = new Mock<ITokenService>();
            _mapperMock = new Mock<IMapper>();

            _service = new AuthService(
                _userManagerMock.Object,
                _tokenServiceMock.Object,
                _mapperMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ThrowsConflict_WhenEmailExists()
        {
            // Arrange
            var dto = new RegisterRequestDto { Email = "existing@test.com" };
            _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(new User());

            // Act
            Func<Task> act = () => _service.RegisterAsync(dto);

            // Assert
            await act.Should().ThrowAsync<ConflictException>().WithMessage("Email is already registered.");
        }

        [Fact]
        public async Task RegisterAsync_ReturnsAuthResponse_WhenSuccessful()
        {
            // Arrange
            var dto = new RegisterRequestDto
            {
                Email = "new@test.com",
                UserName = "newuser",
                Password = "Password123",
                Role = Role.User
            };

            _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _tokenServiceMock.Setup(s => s.GenerateAccessToken(It.IsAny<User>()))
                .Returns(("fake-token", DateTime.UtcNow.AddHours(1)));

            // Act
            var result = await _service.RegisterAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.AccessToken.Should().Be("fake-token");
            _userManagerMock.Verify(m => m.CreateAsync(It.IsAny<User>(), dto.Password), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ThrowsBadRequest_WhenIdentityFails()
        {
            // Arrange
            var dto = new RegisterRequestDto { Email = "fail@test.com", UserName = "failuser", Password = "123" };
            _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            // Act
            Func<Task> act = () => _service.RegisterAsync(dto);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task LoginAsync_ReturnsAdminResponse_WhenHardcodedAdminUsed()
        {
            // Arrange
            var dto = new LoginRequestDto { Email = "admin@sport.com", Password = "Admin@123" };
            var adminUser = new User { Email = "admin@sport.com", Role = Role.Admin };
            _userManagerMock.Setup(m => m.FindByEmailAsync("admin@sport.com")).ReturnsAsync(adminUser);
            _tokenServiceMock.Setup(s => s.GenerateAccessToken(It.IsAny<User>()))
                .Returns(("admin-token", DateTime.UtcNow.AddHours(1)));

            // Act
            var result = await _service.LoginAsync(dto);

            // Assert
            result.AccessToken.Should().Be("admin-token");
        }

        [Fact]
        public async Task LoginAsync_ThrowsUnauthorized_WhenPasswordInvalid()
        {
            // Arrange
            var dto = new LoginRequestDto { Email = "user@test.com", Password = "wrong" };
            var user = new User { Email = "user@test.com" };
            _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

            // Act
            Func<Task> act = () => _service.LoginAsync(dto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task LoginAsync_ReturnsResponse_WhenSuccessful()
        {
            // Arrange
            var dto = new LoginRequestDto { Email = "user@test.com", Password = "Correct123!" };
            var user = new User { Email = "user@test.com" };
            _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
            _tokenServiceMock.Setup(s => s.GenerateAccessToken(It.IsAny<User>()))
                .Returns(("valid-token", DateTime.UtcNow.AddHours(1)));

            // Act
            var result = await _service.LoginAsync(dto);

            // Assert
            result.AccessToken.Should().Be("valid-token");
        }
    }
}
