using Application.Features.Auth.Commands.Register;
using Domain.Constants;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(UserManager<ApplicationUser> userManager)
    {
        RuleFor(x => x.Gender)
            .Must(gender => GenderStatus.IsValid(gender))
            .WithMessage("Gender not vaild. Please check again.");

        RuleFor(x => x.Username)
            .MustAsync(async (username, cancellation) =>
            {
                var existingUser = await userManager.FindByNameAsync(username);
                return existingUser is null;
            })
            .WithMessage("Username already exists.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, cancellation) =>
            {
                var existingUser = await userManager.FindByEmailAsync(email);
                return existingUser is null;
            })
            .WithMessage("Email already exists.");
    }
}
