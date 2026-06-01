using Application.ApiContracts.InventoryReceipt.Requests;
using FluentValidation;

namespace Application.Features.InventoryReceipts.Commands.CreateInventoryReceipt
{
    public sealed class CreateInventoryReceiptInfoCommandValidator : AbstractValidator<CreateInventoryReceiptInfoRequest>
    {
        public CreateInventoryReceiptInfoCommandValidator()
        {
            RuleFor(x => x.PurchaseOrderItemId).GreaterThan(0).When(x => x.PurchaseOrderItemId.HasValue);
            RuleFor(x => x.Count).NotNull().GreaterThan(0);
        }
    }
}
