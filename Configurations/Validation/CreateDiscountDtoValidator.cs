using FluentValidation;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Configurations.Validation
{
    public class CreateDiscountDtoValidator : AbstractValidator<CreateDiscountDto>
    {
        public CreateDiscountDtoValidator()
        {
            RuleFor(x => x.PercentOff)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);

            RuleFor(x => x.ValidFrom)
                .NotEmpty();

            RuleFor(x => x.ValidTo)
                .NotEmpty()
                .GreaterThan(x => x.ValidFrom)
                .WithMessage("ValidTo must be after ValidFrom");

            RuleFor(x => x)
                .Must(x => (x.VenueId.HasValue && !x.CourtId.HasValue) || 
                           (!x.VenueId.HasValue && x.CourtId.HasValue))
                .WithMessage("Either VenueId or CourtId must be provided, but not both");
        }
    }
}
