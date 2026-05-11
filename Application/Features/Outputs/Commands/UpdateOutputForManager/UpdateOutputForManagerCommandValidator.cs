using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Outputs.Commands.UpdateOutputForManager;

public sealed class UpdateOutputForManagerCommandValidator : AbstractValidator<UpdateOutputForManagerCommand>
{
    public UpdateOutputForManagerCommandValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("CurrentUserId không du?c d? tr?ng.");
        RuleFor(x => x.CustomerName).NotEmpty().WithMessage("Tên khách hàng không du?c d? tr?ng.");
        RuleFor(x => x.CustomerAddress).NotEmpty().WithMessage("Ð?a ch? khách hàng không du?c d? tr?ng.");
        RuleFor(x => x.CustomerPhone)
            .NotEmpty()
            .WithMessage("S? di?n tho?i không du?c d? tr?ng.")
            .MustBeValidPhoneNumber()
            .WithMessage("Ð?nh d?ng s? di?n tho?i không h?p l?.");
        RuleFor(x => x.OutputInfos).NotEmpty().WithMessage("Ðon xu?t hàng ph?i có ít nh?t m?t s?n ph?m.");
        RuleForEach(x => x.OutputInfos)
            .ChildRules(
                item =>
                {
                    item.RuleFor(i => i.ProductId).NotEmpty().GreaterThan(0);
                    item.RuleFor(i => i.Count).NotEmpty().GreaterThan(0);
                });
    }
}

