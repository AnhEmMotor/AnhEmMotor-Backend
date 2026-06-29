using FluentValidation;

namespace Application.Features.Maintenances.Commands.CreateMaintenanceTicket
{
    public class CreateMaintenanceTicketCommandValidator : AbstractValidator<CreateMaintenanceTicketCommand>
    {
        public CreateMaintenanceTicketCommandValidator()
        {
            RuleFor(x => x.LicensePlate).NotEmpty().WithMessage("Biển số xe không được trống.");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Mô tả bảo trì không được trống.");
            RuleFor(x => x.Mileage).GreaterThanOrEqualTo(0).WithMessage("Số km (ODO) không được âm.");
            RuleFor(x => x.LaborCost).GreaterThanOrEqualTo(0).WithMessage("Phí nhân công không được âm.");
        }
    }
}
