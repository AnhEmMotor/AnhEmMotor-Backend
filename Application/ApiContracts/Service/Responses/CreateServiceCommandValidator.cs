using FluentValidation;

namespace Application.ApiContracts.Service.Responses;

public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tên dịch vụ không được để trống.")
            .MaximumLength(200)
            .WithMessage("Tên dịch vụ không được vượt quá 200 ký tự.");
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0).WithMessage("Giá dịch vụ cơ bản không được nhỏ hơn 0.");
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Danh mục dịch vụ là bắt buộc.");
    }
}