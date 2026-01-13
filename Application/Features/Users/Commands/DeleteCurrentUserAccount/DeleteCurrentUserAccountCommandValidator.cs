using FluentValidation;

namespace Application.Features.Users.Commands.DeleteCurrentUserAccount;

public sealed class DeleteCurrentUserAccountCommandValidator : AbstractValidator<DeleteCurrentUserAccountCommand>
{
    public DeleteCurrentUserAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is missing.")
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("Invalid User ID format.");
    }
}