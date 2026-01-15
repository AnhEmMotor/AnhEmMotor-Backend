using FluentValidation;
using Application;
using Application.Features;
using Application.Features.Users;
using Application.Features.Users.Commands;

namespace Application.Features.Users.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is missing.")
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("Invalid User ID format.");

        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required.")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters.")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New password cannot be the same as the current password.");
    }
}
