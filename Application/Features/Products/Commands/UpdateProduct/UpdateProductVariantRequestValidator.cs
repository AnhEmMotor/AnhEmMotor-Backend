using Application.ApiContracts.Product.Requests;
using FluentValidation;

namespace Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductVariantRequestValidator : AbstractValidator<UpdateProductVariantRequest>
    {
        public UpdateProductVariantRequestValidator()
        {
            RuleFor(x => x.VariantName)
                .NotEmpty()
                .WithMessage("Variant name is required.")
                .MaximumLength(100)
                .WithMessage("Variant name must not exceed 100 characters.");
            RuleFor(x => x.UrlSlug)
                .NotEmpty()
                .WithMessage("Variant URL slug is required.")
                .When(x => !x.Id.HasValue || x.Id <= 0)
                .MaximumLength(255)
                .WithMessage("URL slug must not exceed 255 characters.")
                .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .When(x => !string.IsNullOrWhiteSpace(x.UrlSlug))
                .WithMessage("URL slug must be lowercase alphanumeric with hyphens only.");
            RuleFor(x => x.Price)
                .NotNull()
                .WithMessage("Variant price is required.")
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");
            RuleFor(x => x.CoverImageUrl)
                .MaximumLength(150)
                .When(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl))
                .WithMessage("Cover image URL must not exceed 150 characters.");
            RuleFor(x => x.CoverImageUrl)
                .NotEmpty()
                .When(x => !HasSpecializedColors(x))
                .WithMessage("Biến thể không có màu sắc riêng phải có cover_image_url.");
            RuleFor(x => x.Colors)
                .Must(
                    colors => colors.Select(c => c.ColorName?.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() ==
                        colors.Count)
                .When(x => x.Colors.Count > 0)
                .WithMessage("Tên màu sắc trong cùng biến thể không được trùng lặp.");
            RuleForEach(x => x.Colors)
                .ChildRules(
                    color =>
                    {
                        color.RuleFor(x => x.ColorName).NotEmpty().WithMessage("Tên màu sắc là bắt buộc.");
                        color.RuleFor(x => x.ColorCode).NotEmpty().WithMessage("Mã màu sắc là bắt buộc.");
                        color.RuleFor(x => x.CoverImageUrl).NotEmpty().WithMessage("Hình ảnh màu sắc là bắt buộc.");
                    });
        }

        private static bool HasSpecializedColors(UpdateProductVariantRequest variant)
        {
            return variant.Colors.Count > 0;
        }
    }
}
