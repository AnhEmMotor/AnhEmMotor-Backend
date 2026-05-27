using Domain.Constants;
using FluentValidation;
using System.Linq;

namespace Application.Features.Quotations.Commands.CreateQuotation
{
    public sealed class CreateQuotationCommandValidator : AbstractValidator<CreateQuotationCommand>
    {
        public CreateQuotationCommandValidator()
        {
            RuleFor(x => x.SupplierId)
                .GreaterThan(0).When(x => x.SupplierId.HasValue).WithMessage("SupplierId must be greater than 0.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(status => status != null && QuotationType.All.Contains(status))
                .WithMessage($"Status must be one of: {string.Join(", ", QuotationType.All)}");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");

            RuleFor(x => x.Products)
                .NotEmpty().WithMessage("Quotation must contain at least one product.");

            RuleFor(x => x.Products)
                .Must(products =>
                {
                    if (products == null) return true;
                    var keys = products
                        .Select(p => (p.ProductVariantId, p.ProductVarientColorId))
                        .ToList();
                    return keys.Distinct().Count() == keys.Count;
                })
                .WithMessage("Product Variant and Color combination cannot be duplicated in a single quotation.");

            RuleForEach(x => x.Products).SetValidator(new CreateQuotationItemRequestValidator());
        }
    }
}
