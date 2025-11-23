using Domain.Constants;
using FluentValidation;

namespace Application.Features.Suppliers.Commands.CreateSupplier;

public sealed class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Address)
            .MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || SupplierStatus.IsValid(status))
            .WithMessage($"Status must be one of: {string.Join(", ", SupplierStatus.AllowedValues)}");
    }
}
