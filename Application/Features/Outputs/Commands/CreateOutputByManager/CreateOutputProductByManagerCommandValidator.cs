using Application.ApiContracts.Output.Requests;
using FluentValidation;

namespace Application.Features.Outputs.Commands.CreateOutputByManager
{
    public sealed class CreateOutputProductByManagerCommandValidator : AbstractValidator<CreateOutputInfoRequest>
    {
        public CreateOutputProductByManagerCommandValidator()
        {
            RuleFor(x => x.ProductId).NotNull().GreaterThan(0);
            RuleFor(x => x.Count).NotNull().GreaterThan(0);
        }
    }
}