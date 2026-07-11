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
		RuleFor(x => x.NameVi)
			.MaximumLength(255)
			.WithMessage("Tên danh mục (Tiếng Việt) không được vượt quá 255 ký tự.")
			.Must(name => string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(name))
			.When(x => !string.IsNullOrWhiteSpace(x.NameVi));
		RuleFor(x => x.NameEn)
			.MaximumLength(255)
			.WithMessage("Tên danh mục (English) không được vượt quá 255 ký tự.")
			.Must(name => string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(name))
			.When(x => !string.IsNullOrWhiteSpace(x.NameEn));
		RuleFor(x => x.Description)
			.MaximumLength(500)
			.WithMessage("Mô tả (Tiếng Việt) không được vượt quá 500 ký tự.")
			.When(x => !string.IsNullOrWhiteSpace(x.Description));
		RuleFor(x => x.DescriptionEn)
			.MaximumLength(500)
			.WithMessage("Mô tả (English) không được vượt quá 500 ký tự.")
			.When(x => !string.IsNullOrWhiteSpace(x.DescriptionEn));
		RuleFor(x => x.ManagementType)
			.Must(ProductManagementType.IsValid)
			.WithMessage("Loại quản lý không hợp lệ.")
			.When(x => !string.IsNullOrWhiteSpace(x.ManagementType));
	}
}
