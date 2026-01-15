using FluentValidation;
using Application;
using Application.Features;
using Application.Features.UserManager;
using Application.Features.UserManager.Commands;

namespace Application.Features.UserManager.Commands.ChangePasswordByManager;

public class ChangePasswordByManagerCommandValidator : AbstractValidator<ChangePasswordByManagerCommand>
{
    public ChangePasswordByManagerCommandValidator() { throw new NotImplementedException(); }

    public static bool IsStrongPassword(string password) { throw new NotImplementedException(); }

    public static bool MeetsMinimumLength(string password, int minLength = 8) { throw new NotImplementedException(); }

    public static bool HasUpperCase(string password) { throw new NotImplementedException(); }

    public static bool HasLowerCase(string password) { throw new NotImplementedException(); }

    public static bool HasDigit(string password) { throw new NotImplementedException(); }

    public static bool HasSpecialCharacter(string password) { throw new NotImplementedException(); }
}
