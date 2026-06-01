using FluentValidation;

namespace Application.Features.InventoryReceipts.Commands.UpdateInventoryReceipt;

public sealed class UpdateInventoryReceiptCommandValidator : AbstractValidator<UpdateInventoryReceiptCommand>
{
    public UpdateInventoryReceiptCommandValidator()
    {
        RuleFor(x => x.Products).NotEmpty().WithMessage("InventoryReceipt must contain at least one product.");
        RuleFor(x => x.Products)
            .Must(
                products =>
                {
                    var poItemIds = products
                        .Where(p => p.PurchaseOrderItemId.HasValue)
                        .Select(p => p.PurchaseOrderItemId!.Value)
                        .ToList();
                    if (poItemIds.Count != poItemIds.Distinct().Count())
                    {
                        return false;
                    }
                    return true;
                })
            .WithMessage(
                "Purchase Order Items cannot be duplicated in a single InventoryReceipt.");
        RuleForEach(x => x.Products).SetValidator(new UpdateInventoryReceiptInfoCommandValidator());
    }
}
