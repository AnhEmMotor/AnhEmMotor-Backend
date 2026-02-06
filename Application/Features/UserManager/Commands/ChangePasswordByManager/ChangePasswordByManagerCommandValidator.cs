using FluentValidation;

namespace Application.Features.UserManager.Commands.ChangePasswordByManager;

public class ChangePasswordByManagerCommandValidator : AbstractValidator<ChangePasswordByManagerCommand>
{
    public ChangePasswordByManagerCommandValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New Password is required.")
            .Must(IsStrongPassword)
            .WithMessage("Password must be strong.");
    }

    public static bool IsStrongPassword(string? password)
    {
        if(string.IsNullOrEmpty(password))
            return false;
        return MeetsMinimumLength(password) &&
            HasUpperCase(password) &&
            HasLowerCase(password) &&
            HasDigit(password) &&
            HasSpecialCharacter(password);
    }

    public static bool MeetsMinimumLength(string password, int minLength = 8) { return password.Length >= minLength; }

    public static bool HasUpperCase(string password) { return password.Any(char.IsUpper); }

    public static bool HasLowerCase(string password) { return password.Any(char.IsLower); }

    public static bool HasDigit(string password) { return password.Any(char.IsDigit); }

    public static bool HasSpecialCharacter(string password) { return password.Any(ch => !char.IsLetterOrDigit(ch)); }
}
