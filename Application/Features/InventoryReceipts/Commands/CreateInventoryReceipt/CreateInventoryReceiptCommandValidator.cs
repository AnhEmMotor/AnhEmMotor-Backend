using FluentValidation;

namespace Application.Features.InventoryReceipts.Commands.CreateInventoryReceipt;

public sealed class CreateInventoryReceiptCommandValidator : AbstractValidator<CreateInventoryReceiptCommand>
{
    public CreateInventoryReceiptCommandValidator()
    {
        RuleFor(x => x.PurchaseRequestId)
            .GreaterThan(0)
            .When(x => x.PurchaseRequestId.HasValue)
            .WithMessage("PurchaseRequestId must be greater than 0.");
        RuleFor(x => x.Notes).MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
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
            .WithMessage(
                "Purchase Request Items cannot be duplicated in a single InventoryReceipt.");
        RuleForEach(x => x.Products).SetValidator(new CreateInventoryReceiptInfoCommandValidator());
    }
}
