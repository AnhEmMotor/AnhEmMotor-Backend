using Application.ApiContracts.Product.Requests;
using FluentValidation;

namespace Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    private static readonly string[] ColorOptionKeys = ["Color", "Màu sắc"];

    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tên sản phẩm không được để trống.")
            .MaximumLength(255)
            .WithMessage("Tên sản phẩm không được vượt quá 255 ký tự.");
        RuleFor(x => x.BrandId)
            .NotNull()
            .WithMessage("Brand Id is required.")
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
        RuleFor(x => x.FrontTireSize)
            .Matches(@"^\d+/\d+-\d+$")
            .When(x => !string.IsNullOrWhiteSpace(x.FrontTireSize))
            .WithMessage("Định dạng lốp trước không hợp lệ (VD: 120/70-17).");
        RuleFor(x => x.RearTireSize)
            .Matches(@"^\d+/\d+-\d+$")
            .When(x => !string.IsNullOrWhiteSpace(x.RearTireSize))
            .WithMessage("Định dạng lốp sau không hợp lệ (VD: 120/70-17).");
        RuleFor(x => x.Variants)
            .NotEmpty()
            .WithMessage("Sản phẩm phải có ít nhất một biến thể.")
            .Must(NotContainColorOptionValues)
            .WithMessage(
                "Không được gửi màu sắc trong optionValues. Vui lòng dùng color_name và color_code của biến thể.")
            .Must(HaveUniqueSlugs)
            .WithMessage("Đã tìm thấy các đường dẫn (slug) trùng lặp trong yêu cầu.")
            .Custom(ValidateVariantOptions);
        RuleForEach(x => x.Variants).SetValidator(new UpdateProductVariantRequestValidator());
    }

    private bool HaveUniqueSlugs(List<UpdateProductVariantRequest> variants)
    {
        if (variants == null)
            return true;
        var slugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return variants.All(v => string.IsNullOrWhiteSpace(v.UrlSlug) || slugs.Add(v.UrlSlug.Trim()));
    }

    private void ValidateVariantOptions(
        List<UpdateProductVariantRequest> variants,
        ValidationContext<UpdateProductCommand> context)
    {
        if (variants == null || variants.Count <= 1)
            return;
        var hasVariantWithoutOptions = variants.Any(
            v => (v.OptionValues == null || v.OptionValues.Count == 0) &&
                !HasColor(v) &&
                string.IsNullOrWhiteSpace(v.VariantName));
        if (hasVariantWithoutOptions)
        {
            context.AddFailure(
                "Variants",
                "Khi có nhiều biến thể, tất cả các biến thể phải có thuộc tính phân biệt. Không được để trống thuộc tính.");
            return;
        }
        var optionSignatures = new HashSet<string>();
        foreach (var variant in variants)
        {
            var parts = new List<string>();
            if (variant.OptionValues != null && variant.OptionValues.Count > 0)
            {
                parts.AddRange(
                    variant.OptionValues
                        .OrderBy(kvp => kvp.Key)
                        .Select(kvp => $"{kvp.Key.Trim().ToLowerInvariant()}:{kvp.Value.Trim().ToLowerInvariant()}"));
            }
            if (!string.IsNullOrWhiteSpace(variant.VariantName))
                parts.Add($"specialized_version:{variant.VariantName.Trim().ToLowerInvariant()}");
            var sig = string.Join("|", parts);
            if (!optionSignatures.Add(sig))
            {
                context.AddFailure(
                    "Variants",
                    "Các biến thể không được trùng lặp tổ hợp thuộc tính, màu sắc và phiên bản.");
                return;
            }
        }
    }

    private bool NotContainColorOptionValues(List<UpdateProductVariantRequest> variants)
    {
        return variants == null ||
            variants.All(
                v => v.OptionValues == null ||
                    v.OptionValues.Keys
                        .All(
                            key => !ColorOptionKeys.Contains(
                                            (key ?? string.Empty).Trim(),
                                            StringComparer.OrdinalIgnoreCase)));
    }

    private static bool HasColor(UpdateProductVariantRequest variant)
    {
        return variant.Colors.Count > 0;
    }
}
