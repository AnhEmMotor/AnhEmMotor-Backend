using FluentValidation;

namespace Application.Features.Outputs.Commands.UpdateOutputForManager;

public sealed class UpdateOutputForManagerCommandValidator : AbstractValidator<UpdateOutputForManagerCommand>
{
    public UpdateOutputForManagerCommandValidator()
    {
        RuleFor(x => x.Model.CurrentUserId)
            .NotEmpty().WithMessage("CurrentUserId không được để trống.");

        RuleFor(x => x.Model).NotNull();

        RuleFor(x => x.Model.OutputInfos)
            .NotEmpty().WithMessage("Đơn xuất hàng phải có ít nhất một sản phẩm.");

        RuleForEach(x => x.Model.OutputInfos).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().GreaterThan(0);
            item.RuleFor(i => i.Count).NotEmpty().GreaterThan(0);
        });
    }
}
