using Application.ApiContracts.Product;
using FluentValidation;

namespace Application.Features.Products.Commands.CreateProduct
{
    public sealed class CreateProductVariantValidator : AbstractValidator<ProductVariantWriteRequest>
    {
        public CreateProductVariantValidator()
        {
            RuleFor(x => x.UrlSlug)
                .NotEmpty()
                .WithMessage("Variant URL slug is required.")
                .MaximumLength(50)
                .WithMessage("URL slug must not exceed 50 characters.")
                .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("URL slug must be lowercase alphanumeric with hyphens only.");

            RuleFor(x => x.Price)
                .NotNull()
                .WithMessage("Variant price is required.")
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");

            RuleFor(x => x.CoverImageUrl)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl))
                .WithMessage("Cover image URL must not exceed 100 characters.");

            RuleFor(x => x.OptionValueIds).NotNull().WithMessage("Option values must be specified (can be empty list).");
        }
    }
}
