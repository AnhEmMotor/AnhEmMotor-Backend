using Domain.Constants.Product;
using FluentValidation;

namespace Application.Features.ProductCategories.Commands.UpdateProductCategory;

public class UpdateProductCategoryCommandValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID danh m?c là b?t bu?c.")
            .GreaterThan(0)
            .WithMessage("ID danh m?c ph?i là s? duong.");
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Tên danh m?c không du?c vu?t quá 100 ký t?.")
            .Must(name => name == null || !string.IsNullOrWhiteSpace(name))
            .WithMessage("Tên danh m?c không du?c ch? ch?a kho?ng tr?ng.")
            .When(x => x.Name != null);
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Mô t? không du?c vu?t quá 500 ký t?.")
            .When(x => x.Description != null);
        RuleFor(x => x.ManagementType)
            .Must(ProductManagementType.IsValid)
            .WithMessage("Lo?i qu?n lý không h?p l?.")
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
            .WithMessage("Ph?i cung c?p ít nh?t m?t tru?ng d? c?p nh?t.");
    }
}
