using FluentValidation;

namespace Application.Features.ProductCategories.Commands.UpdateProductCategory;

public sealed class UpdateProductCategoryCommandValidator : AbstractValidator<UpdateProductCategoryCommand>
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
        RuleFor(x => x)
            .Must(x => x.Name != null || x.Description != null)
            .WithMessage("Ph?i cung c?p ít nh?t Tên ho?c Mô t? d? c?p nh?t.");
    }
}
