using FluentValidation;

namespace Application.Features.Products.Queries.CheckSlugAvailability;

public sealed class CheckSlugAvailabilityQueryValidator : AbstractValidator<CheckSlugAvailabilityQuery>
{
    public CheckSlugAvailabilityQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Slug cannot be empty.")
            .MaximumLength(50)
            .WithMessage("Slug must not exceed 50 characters.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens only.");
    }
}
