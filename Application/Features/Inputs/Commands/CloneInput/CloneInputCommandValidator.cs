using FluentValidation;

namespace Application.Features.InventoryReceipts.Commands.CloneInput;

public sealed class CloneInputCommandValidator : AbstractValidator<CloneInputCommand>
{
    public CloneInputCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id phải lớn hơn 0");
    }
}
