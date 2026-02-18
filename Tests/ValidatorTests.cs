using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using Assignment_Example_HU.Configurations.Validation;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Tests.Validators
{
    public class ValidatorTests
    {
        private readonly RegisterRequestValidator _registerValidator;
        private readonly LoginRequestValidator _loginValidator;

        public ValidatorTests()
        {
            _registerValidator = new RegisterRequestValidator();
            _loginValidator = new LoginRequestValidator();
        }

        [Fact]
        public void RegisterValidator_ShouldHaveError_WhenEmailInvalid()
        {
            var model = new RegisterRequestDto { Email = "invalid-email" };
            var result = _registerValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void RegisterValidator_ShouldNotHaveError_WhenDataValid()
        {
            var model = new RegisterRequestDto
            {
                Email = "test@test.com",
                UserName = "testuser",
                Password = "Password123"
            };
            var result = _registerValidator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void LoginValidator_ShouldHaveError_WhenEmailEmpty()
        {
            var model = new LoginRequestDto { Email = "" };
            var result = _loginValidator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void CreateVenueValidator_ShouldHaveError_WhenNameEmpty()
        {
            var validator = new CreateVenueDtoValidator();
            var model = new CreateVenueDto { Name = "" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void CreateCourtValidator_ShouldHaveError_WhenDurationInvalid()
        {
            var validator = new CreateCourtDtoValidator();
            var model = new CreateCourtDto { SlotDurationMinutes = -1 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.SlotDurationMinutes);
        }

        [Fact]
        public void CreateDiscountValidator_ShouldHaveError_WhenPercentInvalid()
        {
            var validator = new CreateDiscountDtoValidator();
            var model = new CreateDiscountDto { PercentOff = 150 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.PercentOff);
        }

        [Fact]
        public void CreateDiscountValidator_ShouldHaveError_WhenBothVenueAndCourtProvided()
        {
            var validator = new CreateDiscountDtoValidator();
            var model = new CreateDiscountDto { VenueId = Guid.NewGuid(), CourtId = Guid.NewGuid(), PercentOff = 10, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(1) };
            var result = validator.TestValidate(model);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public void CreateGameValidator_ShouldHaveError_WhenPlayersInvalid()
        {
            var validator = new CreateGameDtoValidator();
            var model = new CreateGameDto { MaxPlayers = 1, MinPlayers = 5 };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.MinPlayers);
        }
    }
}
