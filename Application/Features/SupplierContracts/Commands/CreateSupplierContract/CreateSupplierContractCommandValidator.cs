using Domain.Constants;
using FluentValidation;

namespace Application.Features.SupplierContracts.Commands.CreateSupplierContract;

public sealed class CreateSupplierContractCommandValidator : AbstractValidator<CreateSupplierContractCommand>
{
    public CreateSupplierContractCommandValidator()
    {
        RuleFor(x => x.ContractNumber)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Số hợp đồng là bắt buộc và không được vượt quá 100 ký tự.");
        RuleFor(x => x.EffectiveDate)
            .NotEqual(default(DateTime))
            .WithMessage("Ngày hiệu lực là bắt buộc.");
        RuleFor(x => x.ExpirationDate)
            .GreaterThanOrEqualTo(x => x.EffectiveDate)
            .When(x => x.ExpirationDate.HasValue)
            .WithMessage("Ngày hết hạn không được trước ngày hiệu lực.");
        RuleFor(x => x.ContractValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Giá trị hợp đồng không được âm.");
        RuleFor(x => x.Status)
            .Must(SupplierContractStatus.IsValid)
            .WithMessage("Trạng thái hợp đồng không hợp lệ.");
        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CreditLimit.HasValue)
            .WithMessage("Hạn mức tín dụng không được âm.");
        RuleFor(x => x.PaymentWindowDays)
            .GreaterThan(0)
            .When(x => x.PaymentWindowDays.HasValue)
            .WithMessage("Số ngày thanh toán phải lớn hơn 0.");
        RuleFor(x => x.MinimumVolumePerMonth)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinimumVolumePerMonth.HasValue)
            .WithMessage("Sản lượng tối thiểu không được âm.");
        RuleFor(x => x.DiscountRate)
            .InclusiveBetween(0, 100)
            .When(x => x.DiscountRate.HasValue)
            .WithMessage("Tỷ lệ chiết khấu phải nằm trong khoảng 0 đến 100%.");
        RuleFor(x => x.ContractItems)
            .Must(items => items.Select(item => item.ProductVariantId).Distinct().Count() == items.Count)
            .WithMessage("Sản phẩm trong hợp đồng không được trùng.");
        RuleForEach(x => x.ContractItems).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductVariantId)
                .GreaterThan(0)
                .WithMessage("Sản phẩm trong hợp đồng không hợp lệ.");
            item.RuleFor(x => x.WholesalePrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Giá sỉ không được âm.");
        });
    }
}
