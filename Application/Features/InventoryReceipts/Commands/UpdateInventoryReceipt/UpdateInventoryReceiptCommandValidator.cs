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
                    var priItemIds = products
                        .Where(p => p.PurchaseRequestItemId.HasValue)
                        .Select(p => p.PurchaseRequestItemId!.Value)
                        .ToList();
                    if (priItemIds.Count != priItemIds.Distinct().Count())
                    {
                        return false;
                    }
                    return true;
                })
            .WithMessage("Purchase Request Items cannot be duplicated in a single InventoryReceipt.");
        RuleForEach(x => x.Products).SetValidator(new UpdateInventoryReceiptInfoCommandValidator());
    }
}
