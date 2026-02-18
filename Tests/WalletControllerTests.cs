using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using Assignment_Example_HU.Controllers;
using Assignment_Example_HU.DTOs;
using Assignment_Example_HU.Services.Interfaces;

namespace Assignment_Example_HU.Tests.Controllers
{
    public class WalletControllerTests
    {
        private readonly Mock<IWalletService> _walletServiceMock;
        private readonly WalletController _controller;

        public WalletControllerTests()
        {
            _walletServiceMock = new Mock<IWalletService>();
            _controller = new WalletController(_walletServiceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task GetWallet_ReturnsOk()
        {
            // Arrange
            var wallet = new WalletDto { Balance = 100 };
            _walletServiceMock.Setup(s => s.GetWalletAsync(It.IsAny<Guid>())).ReturnsAsync(wallet);

            // Act
            var result = await _controller.GetWallet();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(wallet);
        }

        [Fact]
        public async Task AddFunds_ReturnsOk()
        {
            // Arrange
            var dto = new AddFundsDto { Amount = 50 };
            var transaction = new TransactionDto { Amount = 50 };
            _walletServiceMock.Setup(s => s.AddFundsAsync(It.IsAny<Guid>(), dto)).ReturnsAsync(transaction);

            // Act
            var result = await _controller.AddFunds(dto);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(transaction);
        }
    }
}
