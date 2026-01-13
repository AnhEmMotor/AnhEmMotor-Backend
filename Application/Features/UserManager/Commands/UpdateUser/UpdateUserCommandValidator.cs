using Domain.Constants;
using FluentValidation;

namespace Application.Features.UserManager.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator() { throw new NotImplementedException(); }

    public static bool IsValidEmail(string email) { throw new NotImplementedException(); }

    public static bool IsValidPhoneNumber(string phoneNumber) { throw new NotImplementedException(); }

    public static bool IsValidGender(string gender) { throw new NotImplementedException(); }

    public static IReadOnlyList<string> ValidGenders => new[]
    {
        GenderStatus.Male,
        GenderStatus.Female,
        GenderStatus.Other
    };
}
