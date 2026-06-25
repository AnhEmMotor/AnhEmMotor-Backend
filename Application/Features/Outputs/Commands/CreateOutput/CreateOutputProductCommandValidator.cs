using Application.ApiContracts.Output.Requests;
using FluentValidation;

namespace Application.Features.Outputs.Commands.CreateOutput
{
    public class CreateOutputProductCommandValidator : AbstractValidator<CreateOutputInfoRequest>
    {
        public CreateOutputProductCommandValidator()
        {
            RuleFor(x => x.ProductVariantId).NotNull().GreaterThan(0);
            RuleFor(x => x.Count).NotNull().GreaterThan(0);
        }
    }
}
