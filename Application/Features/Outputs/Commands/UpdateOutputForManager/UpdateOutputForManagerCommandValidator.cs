using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Outputs.Commands.UpdateOutputForManager;

public sealed class UpdateOutputForManagerCommandValidator : AbstractValidator<UpdateOutputForManagerCommand>
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


        RuleFor(x => x.OutputInfos).NotEmpty().WithMessage("Ðon xu?t hàng ph?i có ít nh?t m?t s?n ph?m.");

        RuleForEach(x => x.OutputInfos)
            .ChildRules(
                item =>
                {
                    item.RuleFor(i => i.ProductId).NotEmpty().GreaterThan(0);
                    item.RuleFor(i => i.Count).NotEmpty().GreaterThan(0);
                });
    }
}

