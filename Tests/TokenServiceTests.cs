using System;
using Microsoft.Extensions.Configuration;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Services;
using Assignment_Example_HU.Enums;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Assignment_Example_HU.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly IConfiguration _configuration;
        private readonly TokenService _service;

        public TokenServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string?> {
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Jwt:SecretKey", "SuperSecretKey1234567890ValueLongEnough32Bytes!"},
                {"Jwt:AccessTokenExpirationMinutes", "60"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _service = new TokenService(_configuration);
        }

        [Fact]
        public void GenerateAccessToken_ReturnsValidTokenString()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
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
            jwtToken.Audiences.Should().Contain("TestAudience");
            jwtToken.Subject.Should().Be(user.Id.ToString());
        }
    }
}
