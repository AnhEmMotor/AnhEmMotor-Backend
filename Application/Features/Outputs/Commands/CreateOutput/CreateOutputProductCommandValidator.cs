using Application.ApiContracts.Output.Requests;
using FluentValidation;

namespace Application.Features.Outputs.Commands.CreateOutput
{
    public sealed class CreateOutputProductCommandValidator : AbstractValidator<CreateOutputInfoRequest>
    {
        public CreateOutputProductCommandValidator()
        {
            RuleFor(x => x.ProductId).NotNull().GreaterThan(0);
            RuleFor(x => x.Count).NotNull().GreaterThan(0);
        }
    }
}