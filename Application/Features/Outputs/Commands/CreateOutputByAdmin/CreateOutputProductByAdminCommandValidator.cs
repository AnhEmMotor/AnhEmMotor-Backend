using Application.ApiContracts.Output.Responses;
using FluentValidation;

namespace Application.Features.Outputs.Commands.CreateOutput
{
    public sealed class CreateOutputProductByAdminCommandValidator : AbstractValidator<OutputInfoResponse>
    {
        public CreateOutputProductByAdminCommandValidator()
        {
            RuleFor(x => x.ProductId).NotNull().GreaterThan(0);

            RuleFor(x => x.Count).NotNull().GreaterThan((short)0);
        }
    }
}