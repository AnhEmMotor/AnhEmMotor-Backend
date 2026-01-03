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
            .MustAsync(
                async (username, cancellation) =>
                {
                    var existingUser = await userReadRepository.FindUserByUsernameAsync(username, cancellation).ConfigureAwait(false);
                    return existingUser is null;
                })
            .WithMessage("Username already exists.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(
                async (email, cancellation) =>
                {
                    var existingUser = await userReadRepository.FindUserByEmailAsync(email, cancellation).ConfigureAwait(false);
                    return existingUser is null;
                })
            .WithMessage("Email already exists.");
    }
}
