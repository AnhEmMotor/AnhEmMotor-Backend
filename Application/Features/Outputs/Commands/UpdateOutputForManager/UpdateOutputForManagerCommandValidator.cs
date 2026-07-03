using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Outputs.Commands.UpdateOutputForManager;

public class UpdateOutputForManagerCommandValidator : AbstractValidator<UpdateOutputForManagerCommand>
{
    public UpdateOutputForManagerCommandValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("CurrentUserId không được để trống.");
        RuleFor(x => x.CustomerName).NotEmpty().WithMessage("Tên khách hàng không được để trống.");
        RuleFor(x => x.CustomerAddress).NotEmpty().WithMessage("Địa chỉ khách hàng không được để trống.");
        RuleFor(x => x.CustomerPhone)
            .NotEmpty()
            .WithMessage("Số điện thoại không được để trống.")
            .MustBeValidPhoneNumber()
            .WithMessage("Định dạng số điện thoại không hợp lệ.");
        RuleFor(x => x.OutputInfos).NotEmpty().WithMessage("Đơn xuất hàng phải có ít nhất một sản phẩm.");
        RuleForEach(x => x.OutputInfos)
            .ChildRules(
                item =>
                {
                    item.RuleFor(i => i.ProductVariantId).NotEmpty().GreaterThan(0);
                    item.RuleFor(i => i.Count).NotEmpty().GreaterThan(0);
                });
    }
}

