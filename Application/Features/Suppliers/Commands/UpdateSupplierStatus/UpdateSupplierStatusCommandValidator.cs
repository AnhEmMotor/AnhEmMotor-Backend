using Application.ValidationAttributes;
using FluentValidation;

namespace Application.Features.Suppliers.Commands.UpdateSupplierStatus;

public sealed class UpdateSupplierStatusCommandValidator : AbstractValidator<UpdateSupplierStatusCommand>
{
    public UpdateSupplierStatusCommandValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => StatusConstants.SupplierStatus.IsValid(status))
            .WithMessage($"Status must be one of: {string.Join(", ", StatusConstants.SupplierStatus.AllowedValues)}");
    }
}
