using FluentValidation;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Product Id must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .MaximumLength(255)
            .WithMessage("Product name must not exceed 255 characters.");

        RuleFor(x => x.BrandId)
            .GreaterThan(0)
            .When(x => x.BrandId.HasValue)
            .WithMessage("Brand Id must be greater than 0.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .When(x => x.CategoryId.HasValue)
            .WithMessage("Product Category Id must be greater than 0.");

        RuleFor(x => x.Variants).NotEmpty().WithMessage("At least one product variant is required.");
    }
}
