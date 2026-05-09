using FluentValidation;

namespace Application.Features.UserManager.Commands.CreateUserByManager;

public class CreateUserByManagerCommandValidator : AbstractValidator<CreateUserByManagerCommand>
{
    public CreateUserByManagerCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.");
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.");
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters long.");
        RuleFor(x => x.RoleNames).NotEmpty().WithMessage("At least one role must be assigned.");
    }
}
