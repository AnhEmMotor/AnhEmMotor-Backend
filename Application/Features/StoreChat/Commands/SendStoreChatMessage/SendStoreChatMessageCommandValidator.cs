using FluentValidation;

namespace Application.Features.StoreChat.Commands.SendStoreChatMessage;

public class SendStoreChatMessageCommandValidator : AbstractValidator<SendStoreChatMessageCommand>
{
    public SendStoreChatMessageCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Nội dung tin nhắn không được để trống.")
            .MaximumLength(2000)
            .WithMessage("Nội dung tin nhắn tối đa 2000 ký tự.");
    }
}
