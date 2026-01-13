using FluentValidation;

namespace Application.Features.Products.Commands.RestoreManyProducts;

public sealed class RestoreManyProductsCommandValidator : AbstractValidator<RestoreManyProductsCommand>
{
    public RestoreManyProductsCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("Bạn chưa truyền danh sách Product ID để xoá.")
            .Must(ids => ids!.Count <= 50)
            .WithMessage("Không được xoá quá 50 sản phẩm một lần để đảm bảo hiệu năng.");

        RuleForEach(x => x.Ids).NotEmpty().WithMessage("Product ID không hợp lệ.");
    }
}