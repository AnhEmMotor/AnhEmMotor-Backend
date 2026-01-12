using Application.ApiContracts.Output.Requests;
using Application.Features.Outputs.Commands.CreateOutput;
using FluentValidation;

namespace Application.Features.Outputs.Commands.CreateOutputByManager
{
    public sealed class CreateOutputByManagerCommandValidator : AbstractValidator<CreateOutputByManagerCommand>
    {
        public CreateOutputByManagerCommandValidator()
        {
            RuleFor(x => x.Model.OutputInfos).NotEmpty().WithMessage("Input must contain at least one product.");

            RuleFor(x => x.Model.OutputInfos)
                .Must(HaveUniqueProducts)
                .WithMessage("Product ID cannot be duplicated in a single output.");

            RuleForEach(x => x.Model.OutputInfos).SetValidator(new CreateOutputProductByManagerCommandValidator());
        }

        private bool HaveUniqueProducts(List<CreateOutputInfoRequest> products)
        {
            if (products == null)
                return true;

            var productIds = new HashSet<int>();

            foreach (var item in products)
            {
                if (item.ProductId.HasValue)
                {
                    if (!productIds.Add(item.ProductId.Value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}