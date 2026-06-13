using FluentValidation;

namespace Application.Features.InventoryReceipts.Commands.CloneInventoryReceipt;

public class CloneInventoryReceiptCommandValidator : AbstractValidator<CloneInventoryReceiptCommand>
{
    public CloneInventoryReceiptCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id phải lớn hơn 0");
    }
}
