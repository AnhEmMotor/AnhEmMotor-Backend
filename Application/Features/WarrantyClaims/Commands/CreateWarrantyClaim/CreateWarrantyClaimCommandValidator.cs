using FluentValidation;

namespace Application.Features.WarrantyClaims.Commands.CreateWarrantyClaim
{
    public class CreateWarrantyClaimCommandValidator : AbstractValidator<CreateWarrantyClaimCommand>
    {
        public CreateWarrantyClaimCommandValidator()
        {
            RuleFor(x => x.LicensePlate)
                .NotEmpty()
                .WithMessage("Biển số xe không được để trống.");

            RuleFor(x => x.IssueDescription)
                .NotEmpty()
                .WithMessage("Mô tả lỗi không được để trống.");
        }
    }
}
