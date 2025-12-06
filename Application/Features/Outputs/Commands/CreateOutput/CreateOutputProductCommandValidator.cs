using Application.ApiContracts.Output.Responses;
using FluentValidation;

namespace Application.Features.Outputs.Commands.CreateOutput
{
    public sealed class CreateOutputProductCommandValidator : AbstractValidator<OutputInfoResponse>
    {
        public CreateOutputProductCommandValidator()
        {
            RuleFor(x => x.ProductId).NotNull().GreaterThan(0);

            RuleFor(x => x.Count).NotNull().GreaterThan((short)0);
        }
    }
}