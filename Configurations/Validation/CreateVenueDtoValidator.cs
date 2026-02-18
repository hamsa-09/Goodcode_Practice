using FluentValidation;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Configurations.Validation
{
    public class CreateVenueDtoValidator : AbstractValidator<CreateVenueDto>
    {
        public CreateVenueDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Address)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(x => x.SportsSupported)
                .NotEmpty()
                .MaximumLength(200);
        }
    }
}
