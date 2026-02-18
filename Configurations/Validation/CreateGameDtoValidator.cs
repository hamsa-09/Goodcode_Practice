using FluentValidation;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Configurations.Validation
{
    public class CreateGameDtoValidator : AbstractValidator<CreateGameDto>
    {
        public CreateGameDtoValidator()
        {
            RuleFor(x => x.SlotId)
                .NotEmpty();

            RuleFor(x => x.MinPlayers)
                .GreaterThan(0)
                .LessThanOrEqualTo(x => x.MaxPlayers)
                .WithMessage("MinPlayers must be less than or equal to MaxPlayers");

            RuleFor(x => x.MaxPlayers)
                .GreaterThan(0)
                .LessThanOrEqualTo(50); // Reasonable max
        }
    }
}
