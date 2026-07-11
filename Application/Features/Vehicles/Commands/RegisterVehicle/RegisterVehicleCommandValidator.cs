using FluentValidation;

namespace Application.Features.Vehicles.Commands.RegisterVehicle;

public class RegisterVehicleCommandValidator : AbstractValidator<RegisterVehicleCommand>
{
    public RegisterVehicleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull().WithMessage("UserId is required.");

        RuleFor(x => x.VinNumber)
            .NotEmpty().WithMessage("VIN cannot be empty.");

        RuleFor(x => x.EngineNumber)
            .NotEmpty().WithMessage("Engine number cannot be empty.");
    }
}
