using FluentValidation;

namespace Application.Features.Vehicles.Commands.CreateVehiclePurchaseHistory;

public class CreateVehiclePurchaseHistoryCommandValidator : AbstractValidator<CreateVehiclePurchaseHistoryCommand>
{
    public CreateVehiclePurchaseHistoryCommandValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0);
        RuleFor(x => x.InvoiceNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SellerName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.PurchaseDate).NotEmpty();
    }
}
