using Application.ApiContracts.InventoryReceipt.Requests;
using FluentValidation;

namespace Application.Features.InventoryReceipts.Commands.UpdateInventoryReceipt
{
    public sealed class UpdateInventoryReceiptInfoCommandValidator : AbstractValidator<UpdateInventoryReceiptInfoRequest>
    {
        public UpdateInventoryReceiptInfoCommandValidator()
        {
            RuleFor(x => x.PurchaseOrderItemId).GreaterThan(0).When(x => x.PurchaseOrderItemId.HasValue);
            RuleFor(x => x.Count).NotNull().GreaterThan(0);
        }
    }
}
