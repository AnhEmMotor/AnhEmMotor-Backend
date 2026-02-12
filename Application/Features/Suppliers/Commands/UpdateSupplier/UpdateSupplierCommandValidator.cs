using FluentValidation;

namespace Application.Features.Suppliers.Commands.UpdateSupplier;

public sealed class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().GreaterThan(0);

        RuleFor(x => x.Phone).Matches(@"^[0-9]*$").MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Email).EmailAddress().MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.TaxIdentificationNumber)
            .Matches(@"^[0-9]*$")
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.TaxIdentificationNumber));

        RuleFor(x => x).Must(HaveAtLeastOneField).WithMessage("At least one field must be provided for update.");
    }

    private bool HaveAtLeastOneField(UpdateSupplierCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.Name) ||
            !string.IsNullOrWhiteSpace(command.Phone) ||
            !string.IsNullOrWhiteSpace(command.Email) ||
            !string.IsNullOrWhiteSpace(command.Address) ||
            !string.IsNullOrWhiteSpace(command.Notes) ||
            !string.IsNullOrWhiteSpace(command.TaxIdentificationNumber);
    }
}