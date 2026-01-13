using FluentValidation;

namespace Application.Features.Users.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryValidator : AbstractValidator<GetCurrentUserQuery>
{
    public GetCurrentUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is missing.")
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("Invalid User ID format.");
    }
}