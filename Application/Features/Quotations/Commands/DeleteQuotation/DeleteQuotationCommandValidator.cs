using FluentValidation;

namespace Application.Features.Quotations.Commands.DeleteQuotation
{
    public sealed class DeleteQuotationCommandValidator : AbstractValidator<DeleteQuotationCommand>
    {
        public DeleteQuotationCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().WithMessage("Yêu cầu Id không được để trống.")
                .GreaterThan(0).WithMessage("Id phải lớn hơn 0.");
        }
    }
}
