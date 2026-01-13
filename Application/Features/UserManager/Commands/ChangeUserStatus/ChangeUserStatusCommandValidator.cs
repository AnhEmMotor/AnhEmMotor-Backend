using Domain.Constants;
using FluentValidation;

namespace Application.Features.UserManager.Commands.ChangeUserStatus;

public sealed class ChangeUserStatusCommandValidator : AbstractValidator<ChangeUserStatusCommand>
{
    public ChangeUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required.")
            .Must(UserStatus.IsValid)
            .WithMessage("Invalid status provided.");
    }
}
