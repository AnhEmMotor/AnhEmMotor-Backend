using FluentValidation;

namespace Application.Features.ProductCategories.Commands.DeleteManyProductCategories;

public sealed class DeleteManyProductCategoriesCommandValidator : AbstractValidator<DeleteManyProductCategoriesCommand>
{
    public DeleteManyProductCategoriesCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("Bạn chưa truyền danh sách Product Category ID để xoá.")
            .Must(ids => ids.Count <= 50)
            .WithMessage("Không được xoá quá 50 danh mục một lần.");

        RuleForEach(x => x.Ids).NotEmpty();
    }
}