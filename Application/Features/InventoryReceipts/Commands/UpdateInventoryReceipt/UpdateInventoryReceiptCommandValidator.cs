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
                    var prItemIds = products
                        .Where(p => p.PurchaseRequestItemId.HasValue)
                        .Select(p => p.PurchaseRequestItemId!.Value)
                        .ToList();
                    if (prItemIds.Count != prItemIds.Distinct().Count())
                    {
                        return false;
                    }
                    var quoteRowIds = products
                        .Where(p => p.QuotationProductRowId.HasValue)
                        .Select(p => p.QuotationProductRowId!.Value)
                        .ToList();
                    if (quoteRowIds.Count != quoteRowIds.Distinct().Count())
                    {
                        return false;
                    }
                    return true;
                })
            .WithMessage(
                "Purchase Request Items or Quotation Product Rows cannot be duplicated in a single InventoryReceipt.");
        RuleForEach(x => x.Products).SetValidator(new UpdateInventoryReceiptInfoCommandValidator());
    }
}
