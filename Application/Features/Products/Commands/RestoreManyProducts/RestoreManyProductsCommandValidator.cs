using FluentValidation;

namespace Application.Features.Products.Commands.RestoreManyProducts;

public sealed class RestoreManyProductsCommandValidator : AbstractValidator<RestoreManyProductsCommand>
{
    public RestoreManyProductsCommandValidator()
    {
        RuleFor(x => x.Ids)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Bạn chưa truyền danh sách Product ID để phục hồi.")
        RuleForEach(x => x.Ids).NotEmpty().WithMessage("Product ID không hợp lệ.");
    }
}