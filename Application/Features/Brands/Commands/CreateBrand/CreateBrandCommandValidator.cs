using FluentValidation;

namespace Application.Features.Brands.Commands.CreateBrand;

public sealed class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Name).Matches(@"^[a-zA-Z0-9\-\s]+$").WithMessage("Brand name contains invalid characters.");

        RuleFor(x => x.Description).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
