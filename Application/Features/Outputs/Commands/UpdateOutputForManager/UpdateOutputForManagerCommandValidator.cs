using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Outputs.Commands.UpdateOutputForManager;

public class UpdateOutputForManagerCommandValidator : AbstractValidator<UpdateOutputForManagerCommand>
{
    public UpdateOutputForManagerCommandValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("CurrentUserId kh?ng du?c d? tr?ng.");
        RuleFor(x => x.CustomerName).NotEmpty().WithMessage("T?n kh?ch h?ng kh?ng du?c d? tr?ng.");
        RuleFor(x => x.CustomerAddress).NotEmpty().WithMessage("??a ch? kh?ch h?ng kh?ng du?c d? tr?ng.");
        RuleFor(x => x.CustomerPhone)
            .NotEmpty()
            .WithMessage("S? di?n tho?i kh?ng du?c d? tr?ng.")
            .MustBeValidPhoneNumber()
            .WithMessage("??nh d?ng s? di?n tho?i kh?ng h?p l?.");
        RuleFor(x => x.OutputInfos).NotEmpty().WithMessage("?on xu?t h?ng ph?i c? ?t nh?t m?t s?n ph?m.");
        RuleForEach(x => x.OutputInfos)
            .ChildRules(
                item =>
                {
                    item.RuleFor(i => i.ProductVariantId).NotEmpty().GreaterThan(0);
                    item.RuleFor(i => i.Count).NotEmpty().GreaterThan(0);
                });
    }
}

