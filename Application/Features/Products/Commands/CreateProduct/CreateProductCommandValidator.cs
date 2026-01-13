using Application.ApiContracts.Product.Requests;
using Application.Features.Products.Commands.CreateProduct;
using FluentValidation;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

        RuleFor(x => x.CategoryId)
            .NotNull().WithMessage("Category ID is required.")
            .GreaterThan(0).WithMessage("Category ID must be greater than 0.");

        RuleFor(x => x.BrandId)
            .GreaterThan(0)
            .When(x => x.BrandId.HasValue)
            .WithMessage("Brand ID must be greater than 0 when provided.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .When(x => x.Weight.HasValue)
            .WithMessage("Weight must be greater than 0.");

        RuleFor(x => x.Variants)
            .NotEmpty().WithMessage("At least one product variant is required.")
            .Must(HaveUniqueSlugs).WithMessage("Duplicate slugs found within the request.");

        RuleForEach(x => x.Variants)
            .SetValidator(new CreateProductVariantCommandValidator());
    }

    private bool HaveUniqueSlugs(List<CreateProductVariantRequest> variants)
    {
        if (variants == null) return true;

        var slugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return variants.All(v =>
            string.IsNullOrWhiteSpace(v.UrlSlug) || slugs.Add(v.UrlSlug.Trim()));
    }
}