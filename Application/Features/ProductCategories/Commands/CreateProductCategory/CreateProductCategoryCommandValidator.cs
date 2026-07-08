using Domain.Constants.Product;
using FluentValidation;

namespace Application.Features.ProductCategories.Commands.CreateProductCategory;

public class CreateProductCategoryCommandValidator : AbstractValidator<CreateProductCategoryCommand>
{
	public CreateProductCategoryCommandValidator()
	{
		RuleFor(x => x.NameVi)
			.NotEmpty()
			.WithMessage("Tên danh mục (Tiếng Việt) không được để trống.")
			.MaximumLength(255)
			.WithMessage("Tên danh mục (Tiếng Việt) không được vượt quá 255 ký tự.");
		RuleFor(x => x.NameEn)
			.NotEmpty()
			.WithMessage("Tên danh mục (English) không được để trống.")
			.MaximumLength(255)
			.WithMessage("Tên danh mục (English) không được vượt quá 255 ký tự.");
		RuleFor(x => x.Description)
			.MaximumLength(500)
			.WithMessage("Mô tả (Tiếng Việt) không được vượt quá 500 ký tự.")
			.When(x => !string.IsNullOrWhiteSpace(x.Description));
		RuleFor(x => x.DescriptionEn)
			.MaximumLength(500)
			.WithMessage("Mô tả (English) không được vượt quá 500 ký tự.")
			.When(x => !string.IsNullOrWhiteSpace(x.DescriptionEn));
		RuleFor(x => x.ManagementType)
			.NotEmpty()
			.Must(ProductManagementType.IsValid)
			.WithMessage("Loại quản lý không hợp lệ.");
	}
}
