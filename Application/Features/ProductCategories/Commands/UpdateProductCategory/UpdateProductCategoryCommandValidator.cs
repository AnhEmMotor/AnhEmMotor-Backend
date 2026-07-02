using Domain.Constants.Product;
using FluentValidation;

namespace Application.Features.ProductCategories.Commands.UpdateProductCategory;

public class UpdateProductCategoryCommandValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID danh mục là bắt buộc.")
            .GreaterThan(0)
            .WithMessage("ID danh mục phải là số dương.");
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Tên danh mục không được vượt quá 100 kí tự.")
            .Must(name => name == null || !string.IsNullOrWhiteSpace(name))
            .WithMessage("Tên danh mục không được chỉ chứa khoảng trắng.")
            .When(x => x.Name != null);
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("M� t? không được vượt quá 500 kí tự.")
            .When(x => x.Description != null);
        RuleFor(x => x.ManagementType)
            .Must(ProductManagementType.IsValid)
            .WithMessage("Loại quản lý không hợp lệ.")
            .When(x => x.ManagementType != null);
        RuleFor(x => x)
            .Must(
                x => x.Name != null ||
                    x.Description != null ||
                    x.Slug != null ||
                    x.ImageUrl != null ||
                    x.ParentId.HasValue ||
                    x.MaxPurchaseQuantity.HasValue ||
                    x.ManagementType != null)
            .WithMessage("Phải cung cấp ít nhất một trường để cập nhật.");
    }
}
