using Domain.Constants;
using FluentValidation;

namespace Application.Features.UserManager.Commands.ChangeMultipleUsersStatus;

public class ChangeMultipleUsersStatusCommandValidator : AbstractValidator<ChangeMultipleUsersStatusCommand>
{
    public ChangeMultipleUsersStatusCommandValidator()
    {
        throw new NotImplementedException();
    }

    public static bool IsValidStatus(string status)
    {
        throw new NotImplementedException();
    }

    public static IReadOnlyList<string> ValidStatuses => new[]
    {
        UserStatus.Active,
        UserStatus.Banned
    };
}
