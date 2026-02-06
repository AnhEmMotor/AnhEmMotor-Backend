using FluentValidation;

namespace Application.Features.Suppliers.Commands.CreateSupplier;

public sealed class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);

        RuleFor(x => x.Phone).MaximumLength(20).Matches(@"^[0-9]*$").When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Email).EmailAddress().MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Phone) || !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Phone or Email is required");
    }
}