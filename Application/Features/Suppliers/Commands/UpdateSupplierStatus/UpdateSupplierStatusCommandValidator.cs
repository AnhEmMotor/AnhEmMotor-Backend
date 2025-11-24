using Domain.Constants;
using FluentValidation;

namespace Application.Features.Suppliers.Commands.UpdateSupplierStatus;

public sealed class UpdateSupplierStatusCommandValidator : AbstractValidator<UpdateSupplierStatusCommand>
{
    public UpdateSupplierStatusCommandValidator()
    {
        RuleFor(x => x.StatusId)
            .NotEmpty()
            .Must(status => SupplierStatus.IsValid(status))
            .WithMessage($"Status must be one of: {string.Join(", ", SupplierStatus.AllowedValues)}");
    }
}
