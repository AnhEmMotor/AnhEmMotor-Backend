using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed class UpdateOutputCommandValidator : AbstractValidator<UpdateOutputCommand>
{
    public UpdateOutputCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().WithMessage("Tên ngu?i nh?n không du?c d? tr?ng.");
        RuleFor(x => x.CustomerAddress).NotEmpty().WithMessage("Ð?a ch? giao hàng không du?c d? tr?ng.");
        RuleFor(x => x.CustomerPhone)
            .NotEmpty()
            .WithMessage("S? di?n tho?i không du?c d? tr?ng.")
            .MustBeValidPhoneNumber()
            .WithMessage("Ð?nh d?ng s? di?n tho?i Vi?t Nam không h?p l?.");
    }
}
