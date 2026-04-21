using Application.ApiContracts.Product.Requests;
using FluentValidation;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("ID sản phẩm phải lớn hơn 0.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tên sản phẩm không được để trống.")
            .MaximumLength(255)
            .WithMessage("Tên sản phẩm không được vượt quá 255 ký tự.");

        RuleFor(x => x.BrandId)
            .GreaterThan(0)
            .When(x => x.BrandId.HasValue)
            .WithMessage("ID thương hiệu phải lớn hơn 0.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .When(x => x.CategoryId.HasValue)
            .WithMessage("ID danh mục sản phẩm phải lớn hơn 0.");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.ShortDescription))
            .WithMessage("Mô tả ngắn không được vượt quá 255 ký tự.");

        RuleFor(x => x.MetaTitle)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.MetaTitle))
            .WithMessage("Tiêu đề SEO không được vượt quá 100 ký tự.");

        RuleFor(x => x.MetaDescription)
            .MaximumLength(255)
            .When(x => !string.IsNullOrWhiteSpace(x.MetaDescription))
            .WithMessage("Mô tả SEO không được vượt quá 255 ký tự.");

        RuleFor(x => x.Variants)
            .NotEmpty()
            .WithMessage("Sản phẩm phải có ít nhất một biến thể.")
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
                "Khi có nhiều biến thể, tất cả các biến thể phải có thuộc tính phân biệt. Không được để trống thuộc tính.");
            return;
        }

        var optionSignatures = new HashSet<string>();
        foreach(var variant in variants)
        {
            var parts = new List<string>();
            
            // Include dynamic options
            if (variant.OptionValues != null)
            {
                parts.AddRange(variant.OptionValues
                    .OrderBy(kvp => kvp.Key)
                    .Select(kvp => $"{kvp.Key.Trim().ToLowerInvariant()}:{kvp.Value.Trim().ToLowerInvariant()}"));
            }

            // Include specialized fields in the signature
            if (!string.IsNullOrWhiteSpace(variant.ColorName))
                parts.Add($"specialized_color:{variant.ColorName.Trim().ToLowerInvariant()}");
            
            if (!string.IsNullOrWhiteSpace(variant.VersionName))
                parts.Add($"specialized_version:{variant.VersionName.Trim().ToLowerInvariant()}");

            var sig = string.Join("|", parts);
            if(!optionSignatures.Add(sig))
            {
                context.AddFailure("Variants", "Các biến thể không được trùng lặp tổ hợp thuộc tính, màu sắc và phiên bản.");
                return;
            }
        }
    }
}
