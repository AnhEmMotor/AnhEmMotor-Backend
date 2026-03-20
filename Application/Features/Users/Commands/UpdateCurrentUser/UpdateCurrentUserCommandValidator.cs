using Application.Common.Validators;
using Domain.Constants;
using FluentValidation;

namespace Application.Features.Users.Commands.UpdateCurrentUser;

public sealed class UpdateCurrentUserCommandValidator : AbstractValidator<UpdateCurrentUserCommand>
{
    public UpdateCurrentUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is missing.")
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("Invalid User ID format.");

        RuleFor(x => x.Gender)
            .Must(GenderStatus.IsValid)
            .WithMessage("Invalid gender. Please check again.")
            .When(x => !string.IsNullOrEmpty(x.Gender));

        RuleFor(x => x.FullName)
            .MaximumLength(255)
            .WithMessage("Full Name must not exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.FullName));

        RuleFor(x => x.PhoneNumber).MustBeValidPhoneNumber().When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
