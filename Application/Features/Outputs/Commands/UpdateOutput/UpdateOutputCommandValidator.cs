using FluentValidation;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed class UpdateOutputCommandValidator : AbstractValidator<UpdateOutputCommand>
{
    public UpdateOutputCommandValidator()
    {
        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("CurrentUserId không du?c d? tr?ng.");


        RuleFor(x => x.OutputInfos)
            .NotEmpty().WithMessage("Ðon xu?t hàng ph?i có ít nh?t m?t s?n ph?m.");

        RuleForEach(x => x.OutputInfos).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().GreaterThan(0);
            item.RuleFor(i => i.Count).NotEmpty().GreaterThan(0);
        });
    }
}
