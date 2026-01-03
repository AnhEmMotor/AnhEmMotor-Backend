using Application.Features.Inputs.Commands.CreateInput;
using FluentValidation;

namespace Application.Features.Inputs.Commands.UpdateInput
{
    public sealed class UpdateInputProductCommandValidator : AbstractValidator<UpdateInputProductCommand>
    {
        public UpdateInputProductCommandValidator()
        {
            RuleFor(x => x.ProductId).NotNull().GreaterThan(0);

            RuleFor(x => x.Count).NotNull().GreaterThan(0);

            RuleFor(x => x.InputPrice).NotNull().GreaterThanOrEqualTo(0);
        }
    }
}