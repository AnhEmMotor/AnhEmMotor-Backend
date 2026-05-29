using Application.ApiContracts.Output.Requests;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Outputs.Commands.CreateOutputByManager
{
    public sealed class CreateOutputByManagerCommandValidator : AbstractValidator<CreateOutputByManagerCommand>
    {
        public CreateOutputByManagerCommandValidator()
        {
            RuleFor(x => x.OutputInfos).NotEmpty().WithMessage("InventoryReceipt must contain at least one product.");
            RuleFor(x => x.OutputInfos)
                .Must(HaveUniqueProducts)
                .WithMessage("Product ID cannot be duplicated in a single output.");
            RuleForEach(x => x.OutputInfos).SetValidator(new CreateOutputProductByManagerCommandValidator());
            RuleFor(x => x.CustomerName).NotEmpty().WithMessage("Customer name is required.");
            RuleFor(x => x.CustomerAddress).NotEmpty().WithMessage("Customer address is required.");
            RuleFor(x => x.CustomerPhone)
                .NotEmpty()
                .WithMessage("Customer phone is required.")
                .MustBeValidPhoneNumber()
                .WithMessage("Invalid phone number format.");
        }

        private bool HaveUniqueProducts(List<CreateOutputInfoRequest> products)
        {
            if (products == null)
                return true;
            var productIds = new HashSet<int>();
            foreach (var item in products)
            {
                if (item.ProductVariantId.HasValue)
                {
                    if (!productIds.Add(item.ProductVariantId.Value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
