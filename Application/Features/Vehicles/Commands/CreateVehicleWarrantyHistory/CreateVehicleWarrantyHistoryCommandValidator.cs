using FluentValidation;

namespace Application.Features.Vehicles.Commands.CreateVehicleWarrantyHistory;

public class CreateVehicleWarrantyHistoryCommandValidator : AbstractValidator<CreateVehicleWarrantyHistoryCommand>
{
    public CreateVehicleWarrantyHistoryCommandValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0);
        RuleFor(x => x.ProviderName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.PolicyNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartDate).NotEmpty();
    }
}
