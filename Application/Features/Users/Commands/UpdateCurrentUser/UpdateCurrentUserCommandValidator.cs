using Domain.Constants;
using FluentValidation;

namespace Application.Features.Users.Commands.UpdateCurrentUser;
public sealed class UpdateCurrentUserCommandValidator : AbstractValidator<UpdateCurrentUserCommand>
{
    public UpdateCurrentUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is missing.")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Invalid User ID format.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .Must(GenderStatus.IsValid).WithMessage("Invalid gender. Please check again.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(100).WithMessage("Full Name cannot exceed 100 characters.");

        // Các rule khác nhu Phone, DOB...
    }
}
