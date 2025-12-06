using Application.ApiContracts.Output.Responses;
using FluentValidation;

namespace Application.Features.Outputs.Commands.UpdateOutput
{
    public sealed class UpdateOutputProductCommandValidator : AbstractValidator<OutputInfoResponse>
    {
        public UpdateOutputProductCommandValidator()
        {
            RuleFor(x => x.ProductId).NotNull().GreaterThan(0);

            RuleFor(x => x.Count).NotNull().GreaterThan((short)0);
        }
    }
}