using FluentValidation;

namespace Application.Features.Files.Queries.ViewImage;

public sealed class ViewImageQueryValidator : AbstractValidator<ViewImageQuery>
{
    public ViewImageQueryValidator()
    {
        RuleFor(x => x.Width)
            .GreaterThan(0).WithMessage("Width must be greater than 0.")
            .LessThanOrEqualTo(1200).WithMessage("Width exceeds maximum allowed size of 1200 pixels.");

        RuleFor(x => x.StoragePath)
            .NotEmpty().WithMessage("Storage path is required.");
    }
}