using Application.Features.Suppliers.Commands.DeleteManySuppliers;
using FluentValidation;

namespace Application.Features.Suppliers.Commands.RestoreManySuppliers;

public sealed class RestoreManySuppliersCommandValidator : AbstractValidator<RestoreManySuppliersCommand>
{
    public RestoreManySuppliersCommandValidator()
    {
        RuleFor(x => x.Ids)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Bạn chưa truyền danh sách Supplier ID để phục hồi.")
            .Must(ids => ids.Count <= 20)
            .WithMessage("Để đảm bảo an toàn dữ liệu, chỉ được phục hồi tối đa 20 nhà cung cấp một lần.");

        RuleForEach(x => x.Ids).NotEmpty();
    }
}