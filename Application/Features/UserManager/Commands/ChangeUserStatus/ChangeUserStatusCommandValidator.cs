using Application.Features.UserManager.Commands.ChangeUserStatus;
using Domain.Constants;
using FluentValidation;

public sealed class ChangeUserStatusCommandValidator : AbstractValidator<ChangeUserStatusCommand>
{
    public ChangeUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Model.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(UserStatus.IsValid).WithMessage("Invalid status provided.");
    }
}