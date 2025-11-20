using FluentValidation;

namespace Application.Features.Products.Commands.UpdateManyProductPrices;

public sealed class UpdateManyProductPricesCommandValidator : AbstractValidator<UpdateManyProductPricesCommand>
{
    public UpdateManyProductPricesCommandValidator()
    {
        RuleFor(x => x.ProductPrices)
            .NotEmpty().WithMessage("Product prices dictionary cannot be empty.");

        RuleForEach(x => x.ProductPrices)
            .ChildRules(price =>
            {
                price.RuleFor(p => p.Value)
                    .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative.");
            });
    }
}
