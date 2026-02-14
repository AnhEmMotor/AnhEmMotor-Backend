using Application.ApiContracts.Output.Requests;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Outputs.Commands.CreateOutput
{
    public sealed partial class CreateOutputCommandValidator : AbstractValidator<CreateOutputCommand>
    {
        public CreateOutputCommandValidator()
        {
            RuleFor(x => x.OutputInfos).NotEmpty().WithMessage("Input must contain at least one product.");

            RuleFor(x => x.OutputInfos)
                .Must(HaveUniqueProducts)
                .WithMessage("Product ID cannot be duplicated in a single output.");

            RuleForEach(x => x.OutputInfos).SetValidator(new CreateOutputProductCommandValidator());

            RuleFor(x => x.CustomerPhone)
                .MustBeValidPhoneNumber()
                .WithMessage("Invalid phone number format.")
                .When(x => !string.IsNullOrEmpty(x.CustomerPhone));
        }

        private bool HaveUniqueProducts(List<CreateOutputInfoRequest> products)
        {
            if(products == null)
                return true;

            var productIds = new HashSet<int>();

            foreach(var item in products)
            {
                if(item.ProductId.HasValue)
                {
                    if(!productIds.Add(item.ProductId.Value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
