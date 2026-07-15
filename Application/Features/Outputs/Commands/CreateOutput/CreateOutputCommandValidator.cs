using Application.ApiContracts.Output.Requests;
using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.Outputs.Commands.CreateOutput
{
    public sealed partial class CreateOutputCommandValidator : AbstractValidator<CreateOutputCommand>
    {
        public CreateOutputCommandValidator()
        {
            RuleFor(x => x.OutputInfos).NotEmpty().WithMessage("InventoryReceipt must contain at least one product.");
            RuleFor(x => x.OutputInfos)
                .Must(HaveUniqueProducts)
                .WithMessage("Product ID cannot be duplicated in a single output.");
            RuleForEach(x => x.OutputInfos).SetValidator(new CreateOutputProductCommandValidator());
            RuleFor(x => x.BuyerId).NotEmpty().When(x => x.BuyerId.HasValue).WithMessage("Buyer Id cannot be empty.");
            RuleFor(x => x.CustomerName).NotEmpty().WithMessage("Customer name is required.");
            RuleFor(x => x.CustomerAddress).NotEmpty().WithMessage("Customer address is required.");
            RuleFor(x => x.CustomerPhone)
                .NotEmpty()
                .WithMessage("Customer phone is required.")
                .MustBeValidPhoneNumber()
                .WithMessage("Invalid phone number format.");
            RuleFor(x => x.ProvinceId)
                .NotNull()
                .WithMessage("Vui lòng chọn Tỉnh/Thành phố.")
                .GreaterThan(0)
                .WithMessage("Tỉnh/Thành phố không hợp lệ.");
            RuleFor(x => x.WardCode)
                .NotEmpty()
                .WithMessage("Vui lòng chọn Phường/Xã.");
            RuleFor(x => x.CompanyName)
                .NotEmpty()
                .WithMessage("Tên công ty không được để trống.")
                .MaximumLength(200)
                .WithMessage("Tên công ty không được vượt quá 200 ký tự.")
                .When(x => x.IsCompanyInvoice);
            RuleFor(x => x.CompanyAddress)
                .NotEmpty()
                .WithMessage("Địa chỉ công ty không được để trống.")
                .MaximumLength(500)
                .WithMessage("Địa chỉ công ty không được vượt quá 500 ký tự.")
                .When(x => x.IsCompanyInvoice);
            RuleFor(x => x.CompanyTaxCode)
                .NotEmpty()
                .WithMessage("Mã số thuế không được để trống.")
                .Matches(@"^\d{3}$|^\d{10}$|^\d{13}$|^\d{10}-\d{3}$")
                .WithMessage("Mã số thuế không hợp lệ. Nhập đúng 3 chữ số hoặc MST chuẩn (10/13 số).")
                .When(x => x.IsCompanyInvoice);
            RuleFor(x => x.CompanyEmail)
                .EmailAddress()
                .WithMessage("Email nhận hóa đơn không hợp lệ.")
                .MaximumLength(150)
                .WithMessage("Email không được vượt quá 150 ký tự.")
                .When(x => x.IsCompanyInvoice && !string.IsNullOrWhiteSpace(x.CompanyEmail));
            RuleFor(x => x.BudgetCode)
                .MaximumLength(50)
                .WithMessage("Mã đơn vị ngân sách không được vượt quá 50 ký tự.")
                .When(x => x.IsCompanyInvoice && !string.IsNullOrWhiteSpace(x.BudgetCode));
        }

        private bool HaveUniqueProducts(List<CreateOutputInfoRequest> products)
        {
            if (products == null)
                return true;
            var productIds = new HashSet<int>();
            foreach (var item in products)
            {
                if (item.ProductVariantId.HasValue)
                {
                    if (!productIds.Add(item.ProductVariantId.Value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
