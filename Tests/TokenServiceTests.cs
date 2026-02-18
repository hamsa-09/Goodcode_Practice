using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Services;
using Assignment_Example_HU.Enums;
using System.IdentityModel.Tokens.Jwt;

namespace Assignment_Example_HU.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly TokenService _service;

        public TokenServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock.Setup(c => c.GetSection("Jwt")["Issuer"]).Returns("TestIssuer");
            _configurationMock.Setup(c => c.GetSection("Jwt")["Audience"]).Returns("TestAudience");
            _configurationMock.Setup(c => c.GetSection("Jwt")["SecretKey"]).Returns("SuperSecretKey12345678901234567890");
            _configurationMock.Setup(c => c.GetSection("Jwt")["AccessTokenExpirationMinutes"]).Returns("60");

            _service = new TokenService(_configurationMock.Object);
        }

        [Fact]
        public async Task GenerateAccessToken_ReturnsValidToken()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@test.com",
                UserName = "testuser",
                Role = Role.User
            };

            // Act
            var (token, expiresAt) = _service.GenerateAccessToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();
            expiresAt.Should().BeAfter(DateTime.UtcNow);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            jwtToken.Issuer.Should().Be("TestIssuer");
        }
    }
}
