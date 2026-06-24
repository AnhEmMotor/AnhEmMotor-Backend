using Domain.Constants.Product;
using FluentValidation;

namespace Application.Features.ProductCategories.Commands.CreateProductCategory;

public class CreateProductCategoryCommandValidator : AbstractValidator<CreateProductCategoryCommand>
{
    public CreateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tên danh mục không được để trống.")
            .MaximumLength(100)
            .WithMessage("Tên danh mục không được vượt quá 100 ký tự.");
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Mô tả không được vượt quá 500 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
        RuleFor(x => x.ManagementType)
            .NotEmpty()
            .Must(ProductManagementType.IsValid)
            .WithMessage("Loại quản lý không hợp lệ.");
    }
}
