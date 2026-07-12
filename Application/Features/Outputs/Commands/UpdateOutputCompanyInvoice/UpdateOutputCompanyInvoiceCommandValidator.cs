using FluentValidation;

namespace Application.Features.Outputs.Commands.UpdateOutputCompanyInvoice;

public class UpdateOutputCompanyInvoiceCommandValidator : AbstractValidator<UpdateOutputCompanyInvoiceCommand>
{
    public UpdateOutputCompanyInvoiceCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .WithMessage("Tên công ty không được để trống.")
            .MaximumLength(200)
            .WithMessage("Tên công ty không được vượt quá 200 ký tự.");
        RuleFor(x => x.CompanyAddress)
            .NotEmpty()
            .WithMessage("Địa chỉ công ty không được để trống.")
            .MaximumLength(500)
            .WithMessage("Địa chỉ công ty không được vượt quá 500 ký tự.");
        RuleFor(x => x.CompanyTaxCode)
            .NotEmpty()
            .WithMessage("Mã số thuế không được để trống.")
            .Matches(@"^\d{3}$|^\d{10}$|^\d{13}$|^\d{10}-\d{3}$")
            .WithMessage(
                "Mã số thuế không hợp lệ. Nhập đúng 3 chữ số (ví dụ: 123) hoặc định dạng mã số thuế chuẩn (10 hoặc 13 số).");
        RuleFor(x => x.CompanyEmail)
            .EmailAddress()
            .WithMessage("Email nhận hóa đơn không hợp lệ.")
            .MaximumLength(150)
            .WithMessage("Email không được vượt quá 150 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.CompanyEmail));
        RuleFor(x => x.BudgetCode)
            .MaximumLength(50)
            .WithMessage("Mã đơn vị ngân sách không được vượt quá 50 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.BudgetCode));
    }
}
