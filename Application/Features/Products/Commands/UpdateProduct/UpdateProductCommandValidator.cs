using FluentValidation;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Product Id must be greater than 0.");

        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(255).WithMessage("Product name must not exceed 255 characters.");

        RuleFor(x => x.Request.StatusId)
            .Must(status => ValidationAttributes.StatusConstants.ProductStatus.IsValid(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidationAttributes.StatusConstants.ProductStatus.AllowedValues)}.");

        RuleFor(x => x.Request.BrandId)
            .GreaterThan(0).When(x => x.Request.BrandId.HasValue)
            .WithMessage("Brand Id must be greater than 0.");

        RuleFor(x => x.Request.CategoryId)
            .GreaterThan(0).When(x => x.Request.CategoryId.HasValue)
            .WithMessage("Product Category Id must be greater than 0.");

        RuleFor(x => x.Request.Variants)
            .NotEmpty().WithMessage("At least one product variant is required.");
    }
}
