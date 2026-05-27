using Application.ApiContracts.Quotation.Requests;
using FluentValidation;

namespace Application.Features.Quotations.Commands.CreateQuotation
{
    public sealed class CreateQuotationItemRequestValidator : AbstractValidator<CreateQuotationItemRequest>
    {
        public CreateQuotationItemRequestValidator()
        {
            RuleFor(x => x.ProductVariantId)
                .NotEmpty().WithMessage("ProductVariantId is required.")
                .Must(id => int.TryParse(id, out var parsed) && parsed > 0).WithMessage("ProductVariantId must be a valid positive integer.");

            RuleFor(x => x.ProductVarientColorId)
                .Must(id => string.IsNullOrEmpty(id) || (int.TryParse(id, out var parsed) && parsed > 0)).WithMessage("ProductVarientColorId must be a valid positive integer if provided.");

            RuleFor(x => x.QuotePrice)
                .NotNull().WithMessage("QuotePrice is required.")
                .GreaterThanOrEqualTo(0).WithMessage("QuotePrice must be greater than or equal to 0.");
        }
    }
}
