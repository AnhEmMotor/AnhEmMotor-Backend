using FluentValidation;

namespace Application.Features.Outputs.Commands.CreateOutput;

public sealed class CreateOutputCommandValidator : AbstractValidator<CreateOutputCommand>
{
    public CreateOutputCommandValidator()
    {
        RuleFor(x => x.OutputInfos).NotEmpty().WithMessage("Input must contain at least one product.");

        RuleFor(x => x.OutputInfos)
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
            .WithMessage("Product ID cannot be duplicated in a single output.");

        RuleForEach(x => x.OutputInfos).SetValidator(new CreateOutputProductCommandValidator());
    }
}
