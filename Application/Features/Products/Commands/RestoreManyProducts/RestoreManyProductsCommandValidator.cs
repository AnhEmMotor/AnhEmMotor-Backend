using Application.Features.Products.Commands.RestoreManyProducts;
using FluentValidation;

namespace Application.Features.Products.Commands.DeleteManyProducts;

public sealed class RestoreManyProductsCommandValidator : AbstractValidator<RestoreManyProductsCommand>
{
    public RestoreManyProductsCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty().WithMessage("Bạn chưa truyền danh sách Product ID để xoá.")
            .Must(ids => ids.Count <= 50).WithMessage("Không được xoá quá 50 sản phẩm một lần để đảm bảo hiệu năng.");

        // Check từng ID trong list không được rỗng (nếu cần thiết với Guid)
        RuleForEach(x => x.Ids).NotEmpty().WithMessage("Product ID không hợp lệ.");
    }
}