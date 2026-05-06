using Application.ApiContracts.Product.Requests;
using Application.Features.Products.Commands.CreateProduct;
using FluentValidation;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tên sản phẩm không được để trống.")
            .MaximumLength(100)
            .WithMessage("Tên sản phẩm không được vượt quá 100 ký tự.");
        RuleFor(x => x.CategoryId)
            .NotNull()
            .WithMessage("ID danh mục là bắt buộc.")
            .GreaterThan(0)
            .WithMessage("ID danh mục phải lớn hơn 0.");
        RuleFor(x => x.BrandId)
            .NotNull()
            .WithMessage("Brand ID is required.")
            .GreaterThan(0)
            .When(x => x.BrandId.HasValue)
            .WithMessage("ID thương hiệu phải lớn hơn 0 khi cung cấp.");
        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Mô tả không được vượt quá 2000 ký tự.");
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
        RuleFor(x => x.Weight).GreaterThan(0).When(x => x.Weight.HasValue).WithMessage("Khối lượng phải lớn hơn 0.");
        RuleFor(x => x.Variants)
            .NotEmpty()
            .WithMessage("Sản phẩm phải có ít nhất một biến thể.")
            .Must(HaveUniqueSlugs)
            .WithMessage("Đã tìm thấy các đường dẫn (slug) trùng lặp trong yêu cầu.")
            .Custom(ValidateVariantOptions);
        RuleForEach(x => x.Variants).SetValidator(new CreateProductVariantCommandValidator());
    }

    private void ValidateVariantOptions(
        List<CreateProductVariantRequest> variants,
        ValidationContext<CreateProductCommand> context)
    {
        if (variants == null || variants.Count <= 1)
            return;
        var hasVariantWithoutOptions = variants.Any(
            v => (v.OptionValues == null || v.OptionValues.Count == 0) &&
                string.IsNullOrWhiteSpace(v.ColorName) &&
                string.IsNullOrWhiteSpace(v.VersionName));
        if (hasVariantWithoutOptions)
        {
            context.AddFailure(
                "Variants",
                "Khi có nhiều biến thể, tất cả các biến thể phải có thuộc tính phân biệt (Options, Màu sắc hoặc Phiên bản).");
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
            if (!string.IsNullOrWhiteSpace(variant.ColorName))
                parts.Add($"specialized_color:{variant.ColorName.Trim().ToLowerInvariant()}");
            if (!string.IsNullOrWhiteSpace(variant.VersionName))
                parts.Add($"specialized_version:{variant.VersionName.Trim().ToLowerInvariant()}");
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

    private bool HaveUniqueSlugs(List<CreateProductVariantRequest> variants)
    {
        if (variants == null)
            return true;
        var slugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return variants.All(v => string.IsNullOrWhiteSpace(v.UrlSlug) || slugs.Add(v.UrlSlug.Trim()));
    }
}