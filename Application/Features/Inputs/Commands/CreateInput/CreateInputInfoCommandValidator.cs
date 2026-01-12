using Application.ApiContracts.Input.Requests;
using FluentValidation;

namespace Application.Features.Inputs.Commands.CreateInput
{
    public sealed class CreateInputInfoCommandValidator : AbstractValidator<CreateInputInfoRequest>
    {
        public CreateInputInfoCommandValidator()
        {
            RuleFor(x => x.ProductId).NotNull().GreaterThan(0);

            RuleFor(x => x.Count).NotNull().GreaterThan((short)0);

            RuleFor(x => x.InputPrice).NotNull().GreaterThanOrEqualTo(0);
        }
    }
}