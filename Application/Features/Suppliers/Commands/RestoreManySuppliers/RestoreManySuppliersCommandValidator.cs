using FluentValidation;

namespace Application.Features.Suppliers.Commands.RestoreManySuppliers;

public sealed class RestoreManySuppliersCommandValidator : AbstractValidator<RestoreManySuppliersCommand>
{
    public RestoreManySuppliersCommandValidator()
    {
        RuleFor(x => x.Ids)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Bạn chưa truyền danh sách Supplier ID để phục hồi.");
        RuleForEach(x => x.Ids).NotEmpty();
    }
}