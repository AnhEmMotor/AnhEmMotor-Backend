using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed class UpdateOutputCommandValidator : AbstractValidator<UpdateOutputCommand>
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
    }
}
