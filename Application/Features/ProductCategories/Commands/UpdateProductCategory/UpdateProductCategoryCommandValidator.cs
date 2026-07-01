using Domain.Constants.Product;
using FluentValidation;

namespace Application.Features.ProductCategories.Commands.UpdateProductCategory;

public class UpdateProductCategoryCommandValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("ID danh m?c l� b?t bu?c.")
            .GreaterThan(0)
            .WithMessage("ID danh m?c ph?i l� s? duong.");
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("T�n danh m?c kh�ng du?c vu?t qu� 100 k� t?.")
            .Must(name => name == null || !string.IsNullOrWhiteSpace(name))
            .WithMessage("T�n danh m?c kh�ng du?c ch? ch?a kho?ng tr?ng.")
            .When(x => x.Name != null);
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("M� t? kh�ng du?c vu?t qu� 500 k� t?.")
            .When(x => x.Description != null);
        RuleFor(x => x.ManagementType)
            .Must(ProductManagementType.IsValid)
            .WithMessage("Lo?i qu?n l� kh�ng h?p l?.")
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
            .WithMessage("Ph?i cung c?p �t nh?t m?t tru?ng d? c?p nh?t.");
    }
}
