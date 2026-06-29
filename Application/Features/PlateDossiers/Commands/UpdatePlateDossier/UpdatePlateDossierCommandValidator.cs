using FluentValidation;

namespace Application.Features.PlateDossiers.Commands.UpdatePlateDossier
{
    public class UpdatePlateDossierCommandValidator : AbstractValidator<UpdatePlateDossierCommand>
    {
        public UpdatePlateDossierCommandValidator()
        {
            RuleFor(x => x.CustomerName).NotEmpty().WithMessage("Họ tên khách hàng không được trống.");
            RuleFor(x => x.CustomerPhone).NotEmpty().WithMessage("Số điện thoại không được trống.")
                .Matches(@"^(03|05|07|08|09)\d{8}$").WithMessage("Số điện thoại không hợp lệ.");
            RuleFor(x => x.LicensePlate).NotEmpty().WithMessage("Biển số xe không được trống.");
            RuleFor(x => x.VinNumber).NotEmpty().WithMessage("Số khung không được trống.");
        }
    }
}
