using Application.ApiContracts.InventoryReceipt.Requests;
using FluentValidation;

namespace Application.Features.InventoryReceipts.Commands.CreateInventoryReceipt
{
    public sealed class CreateInventoryReceiptInfoCommandValidator : AbstractValidator<CreateInventoryReceiptInfoRequest>
    {
        public CreateInventoryReceiptInfoCommandValidator()
        {
            RuleFor(x => x.PurchaseRequestItemId).GreaterThan(0).When(x => x.PurchaseRequestItemId.HasValue);
            RuleFor(x => x.QuotationProductRowId).GreaterThan(0).When(x => x.QuotationProductRowId.HasValue);
            RuleFor(x => x.Count).NotNull().GreaterThan(0);
        }
    }
}
