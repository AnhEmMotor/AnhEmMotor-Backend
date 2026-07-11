using FluentValidation;

namespace Application.Features.Vehicles.Queries.GetClientVehicleDetail;

public class GetClientVehicleDetailQueryValidator : AbstractValidator<GetClientVehicleDetailQuery>
{
    public GetClientVehicleDetailQueryValidator()
    {
        RuleFor(x => x.VehicleId)
            .GreaterThan(0)
            .WithMessage("VehicleId must be greater than 0.")
            .WithErrorCode("VehicleId");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.")
            .WithErrorCode("UserId");
    }
}
