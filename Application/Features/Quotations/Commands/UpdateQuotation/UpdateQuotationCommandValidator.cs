using FluentValidation;
using System.Linq;

namespace Application.Features.Quotations.Commands.UpdateQuotation
{
    public sealed class UpdateQuotationCommandValidator : AbstractValidator<UpdateQuotationCommand>
    {
        public UpdateQuotationCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotNull()
                .WithMessage("Id is required.")
                .GreaterThan(0)
                .WithMessage("Id must be greater than 0.");
            RuleFor(x => x.SupplierId)
                .GreaterThan(0)
                .When(x => x.SupplierId.HasValue)
                .WithMessage("SupplierId must be greater than 0.");
            RuleFor(x => x.Notes).MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
            RuleFor(x => x.Products).NotEmpty().WithMessage("Quotation must contain at least one product.");
            RuleFor(x => x.Products)
                .Must(
                    products =>
                    {
                        if (products == null)
                            return true;
                        var keys = products
                        .Select(p => (p.ProductVariantId, p.ProductVarientColorId))
                            .ToList();
                        return keys.Distinct().Count() == keys.Count;
                    })
                .WithMessage("Product Variant and Color combination cannot be duplicated in a single quotation.");
            RuleForEach(x => x.Products).SetValidator(new UpdateQuotationItemRequestValidator());
        }
    }
}
