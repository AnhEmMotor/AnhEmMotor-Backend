using Application.ApiContracts.Input.Requests;
using FluentValidation;

namespace Application.Features.Inputs.Commands.CreateInput
{
    public sealed class CreateInputInfoCommandValidator : AbstractValidator<CreateInputInfoRequest>
    {
        public CreateInputInfoCommandValidator()
        {
            RuleFor(x => x.PurchaseRequestItemId).GreaterThan(0).When(x => x.PurchaseRequestItemId.HasValue);
            RuleFor(x => x.QuotationProductRowId).GreaterThan(0).When(x => x.QuotationProductRowId.HasValue);
            RuleFor(x => x.Count).NotNull().GreaterThan(0);
        }
    }
}
