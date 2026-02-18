using AutoMapper;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System;
using Assignment_Example_HU.Mappings;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Tests.Mappings
{
    public class MappingProfileTests
    {
        [Fact]
        public void MappingProfile_UserMapping_IsCorrect()
        {
            // Arrange
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();
            var dto = new RegisterRequestDto { Email = "test@test.com", UserName = "testuser" };

            // Act
            var user = mapper.Map<User>(dto);

            // Assert
            user.Email.Should().Be("test@test.com");
            user.UserName.Should().Be("testuser");
        }

        [Fact]
        public void MappingProfile_VenueMapping_IsCorrect()
        {
            // Arrange
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();
            var venue = new Models.Venue { Name = "V1", Owner = new User { UserName = "Owner1" } };

            // Act
            var dto = mapper.Map<VenueDto>(venue);

            // Assert
            dto.Name.Should().Be("V1");
            dto.OwnerName.Should().Be("Owner1");
        }

        [Fact]
        public void MappingProfile_SlotMapping_IsCorrect()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();
            var slot = new Slot { Id = Guid.NewGuid(), Court = new Court { Name = "C1", Venue = new Models.Venue { Name = "V1" } } };

            var dto = mapper.Map<AvailableSlotDto>(slot);

            dto.CourtName.Should().Be("C1");
            dto.VenueName.Should().Be("V1");
        }

        [Fact]
        public void MappingProfile_GameMapping_IsCorrect()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();
            var game = new Game { MinPlayers = 5, CreatedByUser = new User { UserName = "User1" }, Players = new List<GamePlayer> { new GamePlayer() } };

            var dto = mapper.Map<GameDto>(game);

            dto.CreatedByUserName.Should().Be("User1");
            dto.CurrentPlayerCount.Should().Be(1);
        }

        [Fact]
        public void MappingProfile_RatingMapping_IsCorrect()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();
            var rating = new Rating { Score = 5, RatedBy = new User { UserName = "Rater" }, Venue = new Models.Venue { Name = "V1" } };

            var dto = mapper.Map<RatingDto>(rating);

            dto.RatedByName.Should().Be("Rater");
            dto.VenueName.Should().Be("V1");
        }
    }
}
