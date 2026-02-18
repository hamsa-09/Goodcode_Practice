using System;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Models;
using Assignment_Example_HU.Exceptions;
using System.Net;

namespace Assignment_Example_HU.Tests
{
    public class GeneralTests
    {
        [Fact]
        public void Models_ShouldHaveData()
        {
            var user = new User();
            user.Id = Guid.NewGuid();
            user.UserName = "test";

            var venue = new Venue { Name = "V1" };
            var court = new Court { Name = "C1" };
            var slot = new Slot { StartTime = DateTime.UtcNow };
            var game = new Game { MinPlayers = 2 };
            var wallet = new Wallet { Balance = 100 };
            var rating = new Rating { Score = 5 };
            var refund = new Refund { RefundAmount = 50 };
            var transaction = new Transaction { Amount = 10 };
            var waitlist = new Waitlist { Priority = 1 };
            var discount = new Discount { PercentOff = 10 };

            user.Id.Should().NotBeEmpty();
            venue.Name.Should().Be("V1");
            court.Name.Should().Be("C1");
            slot.StartTime.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
            game.MinPlayers.Should().Be(2);
            wallet.Balance.Should().Be(100);
            rating.Score.Should().Be(5);
            refund.RefundAmount.Should().Be(50);
            transaction.Amount.Should().Be(10);
            waitlist.Priority.Should().Be(1);
            discount.PercentOff.Should().Be(10);
        }

        [Fact]
        public void Exceptions_ShouldWork()
        {
            var notFound = new NotFoundException("Not Found");
            var badRequest = new BadRequestException("Bad Request");
            var conflict = new ConflictException("Conflict");
            var forbidden = new ForbiddenException("Forbidden");

            notFound.StatusCode.Should().Be(HttpStatusCode.NotFound);
            badRequest.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            conflict.StatusCode.Should().Be(HttpStatusCode.Conflict);
            forbidden.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
