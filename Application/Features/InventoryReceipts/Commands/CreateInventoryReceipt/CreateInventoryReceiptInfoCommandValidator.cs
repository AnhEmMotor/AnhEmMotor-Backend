using Application.ApiContracts.InventoryReceipt.Requests;
using FluentValidation;

namespace Application.Features.InventoryReceipts.Commands.CreateInventoryReceipt
{
    public class CreateInventoryReceiptInfoCommandValidator : AbstractValidator<CreateInventoryReceiptInfoRequest>
    {
        public CreateInventoryReceiptInfoCommandValidator()
        {
            RuleFor(x => x.Count).NotNull().GreaterThan(0);
        }
    }
}
