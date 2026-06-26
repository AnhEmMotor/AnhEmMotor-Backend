using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Suppliers.Commands.UpdateSupplier;

public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().GreaterThan(0);
        RuleFor(x => x.PartnerTypeId).NotEmpty().WithMessage("Thiếu loại đối tác.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Thiếu tên đối tác.").MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Thiếu số điện thoại.").MustBeValidPhoneNumber();
        RuleFor(x => x.TaxIdentificationNumber).NotEmpty().WithMessage("Thiếu mã số thuế.").Matches(@"^[0-9]*$").MaximumLength(20);
        RuleFor(x => x.Address).NotEmpty().WithMessage("Thiếu địa chỉ.").MaximumLength(300);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
