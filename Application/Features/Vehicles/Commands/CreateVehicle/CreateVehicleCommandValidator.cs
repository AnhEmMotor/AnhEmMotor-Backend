using FluentValidation;

namespace Application.Features.Vehicles.Commands.CreateVehicle;

public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.LeadId)
            .GreaterThan(0).WithMessage("LeadId is required.");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("ProductId is required.");

        RuleFor(x => x.VinNumber)
            .NotEmpty().WithMessage("VIN cannot be empty.");

        RuleFor(x => x.EngineNumber)
            .NotEmpty().WithMessage("Engine number cannot be empty.");
    }
}
