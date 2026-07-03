using Application.ApiContracts.InventoryReceipt.Requests;
using FluentValidation;

namespace Application.Features.InventoryReceipts.Commands.UpdateInventoryReceipt
{
    public class UpdateInventoryReceiptInfoCommandValidator : AbstractValidator<UpdateInventoryReceiptInfoRequest>
    {
        public UpdateInventoryReceiptInfoCommandValidator()
        {
            RuleFor(x => x.Count).NotNull().GreaterThan(0);
        }
    }
}
