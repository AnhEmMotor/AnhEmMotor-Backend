using FluentValidation;

namespace Application.Features.Vehicles.Commands.CreateVehicleMaintenanceHistory;

public class CreateVehicleMaintenanceHistoryCommandValidator : AbstractValidator<CreateVehicleMaintenanceHistoryCommand>
{
    public CreateVehicleMaintenanceHistoryCommandValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0);
        RuleFor(x => x.MaintenanceDate).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Mileage).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PartsCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LaborCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.NextMaintenanceOdo).GreaterThan(0).When(x => x.NextMaintenanceOdo.HasValue);
    }
}
