using Application.ApiContracts.Product.Requests;
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

        RuleFor(x => x.ShortDescription)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.ShortDescription))
            .WithMessage("Short Description must not exceed 255 characters.");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.MetaTitle))
            .WithMessage("Meta Title must not exceed 100 characters.");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.MetaDescription))
            .WithMessage("Meta Description must not exceed 255 characters.");

        RuleFor(x => x.Variants)
            .NotEmpty()
            .WithMessage("At least one product variant is required.")
            .Custom(ValidateVariantOptions);
    }

    private void ValidateVariantOptions(
        List<UpdateProductVariantRequest> variants,
        ValidationContext<UpdateProductCommand> context)
    {
        if(variants == null || variants.Count <= 1)
            return;

        var hasVariantWithoutOptions = variants.Any(v => v.OptionValues == null || v.OptionValues.Count == 0);
        if(hasVariantWithoutOptions)
        {
            context.AddFailure(
                "Variants",
                "Multiple variants require all variants to have options. Mixed states or empty options are not allowed.");
            return;
        }

        var optionSignatures = new HashSet<string>();
        foreach(var variant in variants)
        {
            if(variant.OptionValues == null)
                continue;
            var sig = string.Join(
                "|",
                variant.OptionValues
                    .OrderBy(kvp => kvp.Key)
                    .Select(kvp => $"{kvp.Key.Trim().ToLowerInvariant()}:{kvp.Value.Trim().ToLowerInvariant()}"));
            if(!optionSignatures.Add(sig))
            {
                context.AddFailure("Variants", "Duplicate option combinations are not allowed within the same product.");
                return;
            }
        }
    }
}
