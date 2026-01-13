using Application.Features.Suppliers.Commands.DeleteManySuppliers;
using FluentValidation;

namespace Application.Features.Suppliers.Commands.RestoreManySuppliers;

public sealed class RestoreManySuppliersCommandValidator : AbstractValidator<DeleteManySuppliersCommand>
{
    public RestoreManySuppliersCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("Bạn chưa truyền danh sách Supplier ID để xoá.")
            .Must(ids => ids.Count <= 20)
            .WithMessage("Để đảm bảo an toàn dữ liệu, chỉ được xoá tối đa 20 nhà cung cấp một lần.");

        RuleForEach(x => x.Ids).NotEmpty();
    }
}