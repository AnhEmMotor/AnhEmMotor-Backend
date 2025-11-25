using FluentValidation;

namespace Application.Features.Products.Commands.UpdateManyProductPrices;

public sealed class UpdateManyProductPricesCommandValidator : AbstractValidator<UpdateManyProductPricesCommand>
{
    public UpdateManyProductPricesCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty().WithMessage("Product IDs list cannot be empty.");

        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative.");
    }
}
