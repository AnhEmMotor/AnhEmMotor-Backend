using Application.ApiContracts.Quotation.Requests;
using FluentValidation;

namespace Application.Features.Quotations.Commands.UpdateQuotation
{
    public sealed class UpdateQuotationItemRequestValidator : AbstractValidator<UpdateQuotationItemRequest>
    {
        public UpdateQuotationItemRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .When(x => x.Id.HasValue)
                .WithMessage("Item Id must be greater than 0 if provided.");
            RuleFor(x => x.ProductVariantId)
                .NotEmpty()
                .WithMessage("ProductVariantId is required.")
                .Must(id => int.TryParse(id, out var parsed) && parsed > 0)
                .WithMessage("ProductVariantId must be a valid positive integer.");
            RuleFor(x => x.ProductVarientColorId)
                .Must(id => string.IsNullOrEmpty(id) || (int.TryParse(id, out var parsed) && parsed > 0))
                .WithMessage("ProductVarientColorId must be a valid positive integer if provided.");
            RuleFor(x => x.QuotePrice)
                .NotNull()
                .WithMessage("QuotePrice is required.")
                .GreaterThanOrEqualTo(0)
                .WithMessage("QuotePrice must be greater than or equal to 0.");
        }
    }
}
