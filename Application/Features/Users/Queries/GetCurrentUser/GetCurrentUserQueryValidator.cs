using Application.Features.Users.Queries.GetCurrentUser;
using FluentValidation;
using Application.Features.Users.Commands.ChangePasswordCurrentUser;
using Application;
using Application.Features;
using Application.Features.Users;
using Application.Features.Users.Commands;

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