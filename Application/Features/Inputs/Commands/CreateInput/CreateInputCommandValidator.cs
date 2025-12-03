using FluentValidation;

namespace Application.Features.Inputs.Commands.CreateInput;

public sealed class CreateInputCommandValidator : AbstractValidator<CreateInputCommand>
{
    public CreateInputCommandValidator()
    {
        RuleFor(x => x.Products).NotEmpty().WithMessage("Input must contain at least one product.");

        RuleFor(x => x.Products)
            .Must(
                products =>
                {
                    var productIds = products
                    .Where(p => p.ProductId.HasValue)
                        .Select(p => p!.ProductId!.Value)
                        .ToList();

                    var distinctCount = productIds.Distinct().Count();

                    var isDuplicate = productIds.Count != distinctCount;

                    return !isDuplicate;
                })
            .WithMessage("Product ID cannot be duplicated in a single input.");

        RuleForEach(x => x.Products).SetValidator(new CreateInputProductCommandValidator());
    }
}
