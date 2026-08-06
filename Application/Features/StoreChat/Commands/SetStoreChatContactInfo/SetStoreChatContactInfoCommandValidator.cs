using Application.Common.Validators;
using FluentValidation;

namespace Application.Features.StoreChat.Commands.SetStoreChatContactInfo;

public class SetStoreChatContactInfoCommandValidator : AbstractValidator<SetStoreChatContactInfoCommand>
{
    public SetStoreChatContactInfoCommandValidator()
    {
        RuleFor(x => x.ContactName).NotEmpty().WithMessage("Vui lòng nhập tên của bạn.");
        RuleFor(x => x.ContactPhone).NotEmpty().WithMessage("Vui lòng nhập số điện thoại.").MustBeValidPhoneNumber();
    }
}
