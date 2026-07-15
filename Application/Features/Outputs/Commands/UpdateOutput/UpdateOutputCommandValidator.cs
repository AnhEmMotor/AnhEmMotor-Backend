using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public class UpdateOutputCommandValidator : AbstractValidator<UpdateOutputCommand>
{
    public UpdateOutputCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().WithMessage("Tên người nhận không được để trống.");
        RuleFor(x => x.CustomerAddress).NotEmpty().WithMessage("Địa chỉ giao hàng không được để trống.");
        RuleFor(x => x.CustomerPhone)
            .NotEmpty()
            .WithMessage("Số điện thoại không được để trống.")
            .MustBeValidPhoneNumber()
            .WithMessage("Định dạng số điện thoại Việt Nam không hợp lệ.");
        RuleFor(x => x.ProvinceId)
            .NotNull()
            .WithMessage("Vui lòng chọn Tỉnh/Thành phố.")
            .GreaterThan(0)
            .WithMessage("Tỉnh/Thành phố không hợp lệ.");
        RuleFor(x => x.WardCode)
            .NotEmpty()
            .WithMessage("Vui lòng chọn Phường/Xã.");
    }
}
