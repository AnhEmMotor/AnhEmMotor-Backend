using Domain.Constants;
using FluentValidation;

namespace Application.Features.UserManager.Commands.ChangeUserStatus;

public class ChangeUserStatusCommandValidator : AbstractValidator<ChangeUserStatusCommand>
{
    public ChangeUserStatusCommandValidator()
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
