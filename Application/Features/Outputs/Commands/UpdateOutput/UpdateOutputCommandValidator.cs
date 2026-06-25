using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public class UpdateOutputCommandValidator : AbstractValidator<UpdateOutputCommand>
{
    public UpdateOutputCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().WithMessage("T�n ngu?i nh?n kh�ng du?c d? tr?ng.");
        RuleFor(x => x.CustomerAddress).NotEmpty().WithMessage("�?a ch? giao h�ng kh�ng du?c d? tr?ng.");
        RuleFor(x => x.CustomerPhone)
            .NotEmpty()
            .WithMessage("S? di?n tho?i kh�ng du?c d? tr?ng.")
            .MustBeValidPhoneNumber()
            .WithMessage("�?nh d?ng s? di?n tho?i Vi?t Nam kh�ng h?p l?.");
    }
}
