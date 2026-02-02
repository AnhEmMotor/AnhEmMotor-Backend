using FluentValidation;

namespace Application.Features.ProductCategories.Commands.UpdateProductCategory;

public sealed class UpdateProductCategoryCommandValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID danh mục là bắt buộc.")
            .GreaterThan(0).WithMessage("ID danh mục phải là số dương.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Tên danh mục không được vượt quá 100 ký tự.")
            .Must(name => name == null || !string.IsNullOrWhiteSpace(name))
            .WithMessage("Tên danh mục không được chỉ chứa khoảng trắng.")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự.")
            .When(x => x.Description != null);

        RuleFor(x => x)
            .Must(x => x.Name != null || x.Description != null)
            .WithMessage("Phải cung cấp ít nhất Tên hoặc Mô tả để cập nhật.");
    }
}