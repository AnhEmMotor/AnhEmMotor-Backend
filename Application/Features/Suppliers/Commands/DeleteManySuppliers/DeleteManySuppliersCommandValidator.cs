using FluentValidation;

namespace Application.Features.Suppliers.Commands.DeleteManySuppliers;

public sealed class DeleteManySuppliersCommandValidator : AbstractValidator<DeleteManySuppliersCommand>
{
    public DeleteManySuppliersCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("Bạn chưa truyền danh sách Supplier ID để xoá.")
            .Must(ids => ids.Count <= 20)
            .WithMessage("Để đảm bảo an toàn dữ liệu, chỉ được xoá tối đa 20 nhà cung cấp một lần.");

        RuleForEach(x => x.Ids).NotEmpty();
    }
}