using FluentValidation;

namespace Application.Features.UserManager.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator() { throw new NotImplementedException(); }

    public static bool IsStrongPassword(string password) { throw new NotImplementedException(); }

    public static bool MeetsMinimumLength(string password, int minLength = 8) { throw new NotImplementedException(); }

    public static bool HasUpperCase(string password) { throw new NotImplementedException(); }

    public static bool HasLowerCase(string password) { throw new NotImplementedException(); }

    public static bool HasDigit(string password) { throw new NotImplementedException(); }

    public static bool HasSpecialCharacter(string password) { throw new NotImplementedException(); }
}
