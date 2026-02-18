using FluentValidation;
using Assignment_Example_HU.DTOs;

namespace Assignment_Example_HU.Configurations.Validation
{
    public class CreateCourtDtoValidator : AbstractValidator<CreateCourtDto>
    {
        public CreateCourtDtoValidator()
        {
            RuleFor(x => x.VenueId)
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.SlotDurationMinutes)
                .GreaterThan(0)
                .LessThanOrEqualTo(480); // Max 8 hours

            RuleFor(x => x.BasePrice)
                .GreaterThanOrEqualTo(0)
                .LessThan(1000000); // Reasonable max

            RuleFor(x => x.OperatingHours)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
