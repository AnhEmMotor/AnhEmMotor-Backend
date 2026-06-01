using FluentValidation;

namespace Application.Features.InventoryReceipts.Commands.CreateInventoryReceipt;

public sealed class CreateInventoryReceiptCommandValidator : AbstractValidator<CreateInventoryReceiptCommand>
{
    public CreateInventoryReceiptCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .GreaterThan(0)
            .When(x => x.PurchaseOrderId.HasValue)
            .WithMessage("PurchaseOrderId must be greater than 0.");
        RuleFor(x => x.Notes).MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
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
        RuleForEach(x => x.Products).SetValidator(new CreateInventoryReceiptInfoCommandValidator());
    }
}
