using FluentValidation;

namespace Application.Features.Auth.Commands.LoginForManager;

public class LoginForManagerCommandValidator : AbstractValidator<LoginForManagerCommand>
{
    public LoginForManagerCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail).NotEmpty().WithMessage("Username or email is required.");

        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}
