using Application.Interfaces.Repositories.Supplier;
using FluentValidation;

namespace Application.Features.Suppliers.Commands.UpdateSupplier;

public sealed class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().GreaterThan(0);

        RuleFor(x => x)
            .Must(HaveAtLeastOneField)
            .WithMessage("At least one field must be provided for update.");
    }

    private bool HaveAtLeastOneField(UpdateSupplierCommand command)
    {
        return !string.IsNullOrWhiteSpace(command.Name) ||
               !string.IsNullOrWhiteSpace(command.Phone) ||
               !string.IsNullOrWhiteSpace(command.Email) ||
               !string.IsNullOrWhiteSpace(command.TaxIdentificationNumber);
    }
}