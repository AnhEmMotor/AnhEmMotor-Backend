using Domain.Constants;
using FluentValidation;

namespace Application.Features.UserManager.Commands.ChangeMultipleUsersStatus;

public sealed class ChangeMultipleUsersStatusCommandValidator : AbstractValidator<ChangeMultipleUsersStatusCommand>
{
    public ChangeMultipleUsersStatusCommandValidator()
    {
        RuleFor(x => x.UserIds)
            .NotEmpty()
            .WithMessage("User list cannot be empty.")
            .Must(ids => ids!.Count <= 50)
            .WithMessage("To ensure performance, limit to 50 users per update.");

        RuleFor(x => x.Status).NotEmpty().Must(UserStatus.IsValid).WithMessage("Invalid status provided.");
    }
}
