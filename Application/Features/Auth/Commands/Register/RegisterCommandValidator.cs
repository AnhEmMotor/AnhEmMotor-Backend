using Application.Interfaces.Repositories.User;
using Domain.Constants;
using FluentValidation;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(IUserReadRepository userReadRepository)
    {
        RuleFor(x => x.Gender)
            .Must(gender => GenderStatus.IsValid(gender))
            .WithMessage("Gender not vaild. Please check again.");

        RuleFor(x => x.Username)
            .NotEmpty()
            .Matches("^[a-zA-Z0-9]+$").WithMessage("Username must contain only letters and numbers.")
            .MustAsync(
                async (username, cancellation) =>
                {
                    var existingUser = await userReadRepository.FindUserByUsernameAsync(username!, cancellation)
                        .ConfigureAwait(false);
                    return existingUser is null;
                })
            .WithMessage("Username already exists.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(
                async (email, cancellation) =>
                {
                    var existingUser = await userReadRepository.FindUserByEmailAsync(email, cancellation)
                        .ConfigureAwait(false);
                    return existingUser is null;
                })
            .WithMessage("Email already exists.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}