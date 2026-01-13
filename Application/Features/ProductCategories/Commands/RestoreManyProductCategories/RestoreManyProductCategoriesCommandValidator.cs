using FluentValidation;

namespace Application.Features.ProductCategories.Commands.RestoreManyProductCategories;

public sealed class RestoreManyProductCategoriesCommandValidator : AbstractValidator<RestoreManyProductCategoriesCommand>
{
    public RestoreManyProductCategoriesCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("Bạn chưa truyền danh sách Product Category ID để khôi phục.")
            .Must(ids => ids.Count <= 50)
            .WithMessage("Không được khôi phục quá 50 danh mục một lần.");

        RuleForEach(x => x.Ids).NotEmpty();
    }
}